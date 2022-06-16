using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Stats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TabletopTweaks.Core.Utilities;
using ResourceUnification.NewComponents;
using static ResourceUnification.NewComponents.ImprovedAbilityResourceCalc;

namespace ResourceUnification.Content
{
    public static class ModifyTools
    {
        public static List<string> FailureLogs = new();
        private static List<LevelScalingUnification> unifications = new();
        private static void NoteFailure(string failure)
        {
            Main.Context.Logger.LogError(failure);
            FailureLogs.Add(failure);
        }

        #region public wizards

        public static bool RegistrationWizard(string key, BlueprintFeatureReference reference, bool fromClass)
        {
            return RegistrationWizardGuts(new ResourceFeatureInfo(key, reference, fromClass));
        }

        public static bool RegistrationWizardForGUID(string key, string guid, bool fromClass)
        {
            var BP = BlueprintTools.GetBlueprintReference<BlueprintFeatureReference>(guid);
            if (BP == null)
            {
                NoteFailure($"Unification Base Wizard failed on {guid} for {key}: no BP");
                return false;
            }
            else return RegistrationWizardGuts(new ResourceFeatureInfo(key, BP, fromClass));

        }

        internal static void Finish()
        {
            foreach (LevelScalingUnification u in unifications)
            {
                BuildExtendedAmountForLevelScaling(u);
            }
        }

        

        #endregion
        #region private wizard bits
        private static bool RegistrationWizardGuts(ResourceFeatureInfo resourceFeatureInfo)
        {
            Main.Context.Logger.LogHeader($"Registering {resourceFeatureInfo.Feature.NameSafe()} in unified resource system, key: {resourceFeatureInfo.Key}");
            var exists = unifications.FirstOrDefault(x => x.Name.Equals(resourceFeatureInfo.Key, StringComparison.OrdinalIgnoreCase));
            if (exists != null)
            {
                return AltResourceRegistrationWizard(exists, resourceFeatureInfo);
            }
            else
            {
                return RegisterBaseResourceWizard(resourceFeatureInfo);
            }

        }
        private static bool RegisterBaseResourceWizard(ResourceFeatureInfo resourceFeatureInfo)
        {
            var resourceBp = resourceFeatureInfo.Feature?.Get();
            var addResource = resourceBp.Components.OfType<AddAbilityResources>().ToList();
            bool defaultIncreasedByStat = false;
            if (addResource.Count() != 1)
            {
                NoteFailure($"Unification Base Wizard failed on {resourceFeatureInfo.Feature.NameSafe()}: wrong AddAbilityResource count");
                return false;
            }
            if (addResource[0].m_Resource.Get().m_MaxAmount.IncreasedByStat)
            {
                defaultIncreasedByStat = true;
            }

            return RegisterBaseResource(resourceFeatureInfo.Key, resourceFeatureInfo.Feature, addResource[0].m_Resource, defaultIncreasedByStat, addResource[0].m_Resource.Get().m_MaxAmount.ResourceBonusStat, resourceFeatureInfo.FromClass);


        }

        private static bool AltResourceRegistrationWizard(LevelScalingUnification u, ResourceFeatureInfo resourceFeatureInfo)
        {
            var resourceBp = resourceFeatureInfo.Feature?.Get();
            
            var addResource = resourceBp.Components.OfType<AddAbilityResources>().ToList();
            
            if (addResource.Count() != 1)
            {
                NoteFailure($"Alt Resource Wizard failed on {resourceFeatureInfo.Feature.NameSafe()}: wrong AddAbilityResource count");
                return false;
            }


            return RegisterAltResource(u, addResource[0].m_Resource, addResource[0].m_Resource.Get().m_MaxAmount.ResourceBonusStat, resourceFeatureInfo.Feature, resourceFeatureInfo.FromClass);
        }

        #endregion







        private static bool RegisterBaseResource(string key, BlueprintFeatureReference baseAdderFeature, BlueprintAbilityResourceReference baseResourceRef, bool usesStat, StatType statType = StatType.Unknown, bool FromClass = true)
        {
            bool keymatch = unifications.Any(x => x.Name.Equals(key, StringComparison.OrdinalIgnoreCase));
            bool baseRefMatch = unifications.Any(x => x.BaseFeature.Equals(baseAdderFeature));
            if (baseRefMatch && keymatch)
            {
                NoteFailure($"Aborting Registration: {baseAdderFeature.NameSafe()} @ {key} already added");
                return false;
            }
            else if (baseRefMatch)
            {
                NoteFailure($"Aborting Registration: {baseAdderFeature.NameSafe()} already added, but at {key}");
                return false;
            }
            else if (keymatch)
            {
                NoteFailure($"Aborting Registration: {baseAdderFeature.NameSafe()} not added, but {key} is in use");
                return false;
            }
            else
            {
                var unification = new LevelScalingUnification(key, baseAdderFeature, baseResourceRef, usesStat, statType);
                unifications.Add(unification);
                var baseResource = baseResourceRef.Get();
                if (usesStat)
                {
                    baseResource.AddComponent<ResourceSourceInfoComponent>(x =>
                    {
                        x.AltStat = statType;
                        x.m_Unlock = baseAdderFeature;
                        x.IsClassFeature = FromClass;
                    });
                    Main.Context.Logger.LogPatch($"Registered {baseAdderFeature.NameSafe()} as {statType.ToString()} scaling unlock for ", baseResource);
                }

                //unification.BaseAmounts.Add(new Tuple<BlueprintAbilityResource.Amount, BlueprintFeatureReference>(Copy(baseResource.m_MaxAmount), baseAdderFeature));


                return true;
            }
        }

      



        private static bool RegisterAltResource(LevelScalingUnification u, BlueprintAbilityResourceReference redirectFrom, StatType statType, BlueprintFeatureReference unlock, bool FromClass)
        {
            bool redirected = false;
            bool altStatted = false;
            if (u.UnifiedResources.Contains(redirectFrom))
            {
                NoteFailure($"{u.Name} already redirects {redirectFrom.Get().Name}");
            }
            else
            {
                redirected = RegisterResourceRedirect(u, unlock, redirectFrom);
            }
            if (u.AltStatUnlocks.ContainsKey(unlock))
            {
                NoteFailure($"{u.Name} already uses {unlock.Get().Name} as an alt stat unlock");
            }
            else
            {
                altStatted = AddAltStatToUnification(u, statType, unlock, FromClass);
            }
            return redirected || altStatted;
        }

        private static bool RegisterResourceRedirect(LevelScalingUnification unifiedResource, BlueprintFeatureReference unlock, BlueprintAbilityResourceReference redirectFrom)
        {
            var fromBP = redirectFrom.Get();
            var existingRedirect = fromBP.Components.OfType<ResourceRedirectComponent>().FirstOrDefault();
            if (existingRedirect != null)
            {
                if (existingRedirect.m_RedirectTo.Equals(unifiedResource.BaseResource))
                {
                    //Main.Context.Logger.Log($"{fromBP.Name} already redirects to {existingRedirect.m_RedirectTo.Get().name}, skipping redundancy");
                }
                else
                {
                    //Main.Context.Logger.Log($"{fromBP.Name} already redirects to {existingRedirect.m_RedirectTo.Get().name}, redirect conflict detected!");
                }
                return false;
            }
            else
            {
                fromBP.AddComponent<ResourceRedirectComponent>(x =>
                {
                    x.m_RedirectTo = unifiedResource.BaseResource;
                });
                Main.Context.Logger.LogPatch($"Added resource redirect to {unifiedResource.BaseResource.NameSafe()} from:", fromBP);
                var baseResource = unifiedResource.BaseResource.Get();
                var altResource = redirectFrom.Get();

                //unifiedResource.BaseAmounts.Add(new Tuple<BlueprintAbilityResource.Amount, BlueprintFeatureReference>(Copy(baseResource.m_MaxAmount), unlock));


                unifiedResource.UnifiedResources.Add(redirectFrom);
                unifiedResource.ResourceFeatures.Add(unlock);
                //Main.Context.Logger.LogPatch($"Unification with {unifiedResource.Name} Applied to", fromBP);
                return true;
            }


        }

        public static void BuildExtendedAmountForLevelScaling(LevelScalingUnification unification)
        {
            //Main.Context.Logger.Log($"Doing full resource unification for {unification.Name}");
            var baseResource = unification.BaseResource.Get();
            var existing = baseResource.Components.OfType<ImprovedAbilityResourceCalc>().FirstOrDefault();
            if (existing == null)
            {
                baseResource.AddComponent<ImprovedAbilityResourceCalc>(x =>
                {
                    x.BaseValue = baseResource.m_MaxAmount.BaseValue;
                    x.MinClassLevelIncrease = baseResource.m_MaxAmount.MinClassLevelIncrease;
                    x.UsesStat = baseResource.m_MaxAmount.IncreasedByStat;
                    x.OtherClassesModifier = baseResource.m_MaxAmount.OtherClassesModifier;


                });
            }
            else
            {
                if (!existing.UsesStat && baseResource.m_MaxAmount.IncreasedByStat)
                {
                    existing.UsesStat = true;
                }
            }
            
            var extendedAmount = baseResource.Components.OfType<ImprovedAbilityResourceCalc>().FirstOrDefault();

            foreach (var featureRef in unification.ResourceFeatures)
            {
                
                var feature = featureRef.Get();
               
                var resource = feature.Components.OfType<AddAbilityResources>().FirstOrDefault()?.m_Resource.Get();
                if (resource == null)
                {
                    //Main.Context.Logger.Log($"Cannot find AddAbilityResources for {feature.Name}, skipping");
                    continue;
                }
               // Main.Context.Logger.Log($"Calculating class by class progression for {feature.name}");
                
                if (resource.m_MaxAmount.IncreasedByLevel)
                {
                    var maps = ExtractDict(resource.m_MaxAmount.m_Class.Where(x=>x != null).Select(x=>x.Get()).ToList(), resource.m_MaxAmount.m_Archetypes.Where(x => x != null).Select(x => x.Get()).ToList());
                    if (maps.Count == 0 && resource.m_MaxAmount.OtherClassesModifier == 0f)
                    {
                        NoteFailure($"{feature.Name} has IncreasedByLevel set but no classes set for that - this could be a blueprint error or a thing I don't understand yet");

                    }
                    else
                    {
                        foreach (var charClass in maps)
                        {
                            //Main.Context.Logger.Log($"Building class entry for {charClass.Key.NameSafe()} on {unification.Name}");
                            ClassEntry classEntry;
                            if (!extendedAmount.classEntries.TryGetValue(charClass.Key, out classEntry))
                            {
                                //Main.Context.Logger.Log($"Built classEntry for {unification.Name} : {charClass.Key.NameSafe()}");
                                classEntry = new ClassEntry(charClass.Key);
                                extendedAmount.classEntries.Add(charClass.Key, classEntry);
                            }


                            if (charClass.Value.Any())
                            {
                                classEntry.archetypeEntries.Add(new ArchetypeEntry
                                {
                                    Archetypes = charClass.Value,
                                    PerLevel = true,
                                    IncreasePerTick = resource.m_MaxAmount.LevelIncrease
                                });
                                //Main.Context.Logger.Log($"Built classEntry for {unification.Name} and added {charClass.Key.NameSafe()}: {charClass.Value.Select(x=>x.NameSafe() + " and ")}");
                            }
                            else
                            {
                                var classToProbe = charClass.Key.Get();

                                if (classToProbe.Progression.LevelEntries.Any(x => x.Features.Any(x => x.ToReference<BlueprintFeatureBaseReference>().Equals(feature.ToReference<BlueprintFeatureBaseReference>()))))
                                {

                                    var removing = classToProbe.Archetypes.Where(x => x.RemoveFeatures.Any(x => x.Features.Any(x => x.ToReference<BlueprintFeatureBaseReference>().Equals(feature.ToReference<BlueprintFeatureBaseReference>()))));
                                    if (classEntry.vanilla != null)
                                    {
                                        NoteFailure($"Found extra non-archetype entries for {classToProbe.name} from {feature.Name} on {unification}");
                                    }
                                    else
                                    {
                                        classEntry.vanilla = new VanillaEntry
                                        {
                                            BlockedArchetypes = removing.Select(x=>x.ToReference<BlueprintArchetypeReference>()).ToList(),
                                            PerLevel = true,
                                            IncreasePerTick = resource.m_MaxAmount.LevelIncrease
                                        };
                                    }



                                }
                                else if (classToProbe.Archetypes.Any(x => x.AddFeatures.Any(x => x.Features.Any(x => x.ToReference<BlueprintFeatureBaseReference>().Equals(feature.ToReference<BlueprintFeatureBaseReference>())))))
                                {
                                    var adding = classToProbe.Archetypes.Where(x => x.AddFeatures.Any(x => x.Features.Any(x => x.ToReference<BlueprintFeatureBaseReference>().Equals(feature.ToReference<BlueprintFeatureBaseReference>()))));
                                    classEntry.archetypeEntries.Add(new ArchetypeEntry
                                    {
                                        Archetypes = adding.Select(x => x.ToReference<BlueprintArchetypeReference>()).ToList(),
                                        PerLevel = true,
                                        IncreasePerTick = resource.m_MaxAmount.LevelIncrease
                                    });
                                    
                                }
                                else if (classToProbe.PrestigeClass)
                                {
                                    classEntry.vanilla = new VanillaEntry
                                    {
                                       
                                        PerLevel = true,
                                        IncreasePerTick = resource.m_MaxAmount.LevelIncrease
                                    };
                                }
                                else
                                {
                                    NoteFailure($"Could not confirm approriateness of entry for {classToProbe.name} from {feature.Name} on {unification}");
                                }
                            }
                        }



                    }



                }
                if (resource.m_MaxAmount.IncreasedByLevelStartPlusDivStep)
                {
                    var maps = ExtractDict(resource.m_MaxAmount.m_ClassDiv.Where(x => x != null).Select(x => x.Get()).ToList(), resource.m_MaxAmount.m_Archetypes.Where(x => x != null).Select(x => x.Get()).ToList());
                    if (maps.Count == 0 && resource.m_MaxAmount.OtherClassesModifier == 0f)
                    {
                        NoteFailure($"{feature.Name} has IncreasedByLevel set but no classes set for that - this could be a blueprint error or a thing I don't understand yet");

                    }
                    else
                    {
                        foreach (var charClass in maps)
                        {
                            //Main.Context.Logger.Log($"Building class entry for {charClass.Key.NameSafe()} on {unification.Name}");
                            ClassEntry classEntry;
                            if (!extendedAmount.classEntries.TryGetValue(charClass.Key, out classEntry))
                            {
                                //Main.Context.Logger.Log($"Built classEntry for {unification.Name} : {charClass.Key.NameSafe()}");
                                classEntry = new ClassEntry(charClass.Key);
                                extendedAmount.classEntries.Add(charClass.Key, classEntry);
                            }


                            if (charClass.Value.Any())
                            {

                                classEntry.archetypeEntries.Add(new ArchetypeEntry
                                {
                                    Archetypes = charClass.Value,
                                    IncreasePerTick = resource.m_MaxAmount.PerStepIncrease,
                                    StartLevel = resource.m_MaxAmount.StartingLevel,
                                    StartIncrease = resource.m_MaxAmount.StartingIncrease,
                                    LevelStep = resource.m_MaxAmount.LevelStep, 
                                    PerLevel = false

                                });
                                //Main.Context.Logger.Log($"Built classEntry for {unification.Name} and added {charClass.Key.NameSafe()}: {charClass.Value.Select(x => x.NameSafe() + " and ")}");
                            }   
                            else
                            {
                                var classToProbe = charClass.Key.Get();

                                if (classToProbe.Progression.LevelEntries.Any(x => x.Features.Any(x => x.ToReference<BlueprintFeatureBaseReference>().Equals(feature.ToReference<BlueprintFeatureBaseReference>()))))
                                {

                                    var removing = classToProbe.Archetypes.Where(x => x.RemoveFeatures.Any(x => x.Features.Any(x => x.ToReference<BlueprintFeatureBaseReference>().Equals(feature.ToReference<BlueprintFeatureBaseReference>()))));
                                    if (classEntry.vanilla != null)
                                    {
                                        NoteFailure($"Found extra non-archetype entries for {classToProbe.name} from {feature.Name} on {unification}");
                                    }
                                    else
                                    {
                                        classEntry.vanilla = new VanillaEntry
                                        {
                                            BlockedArchetypes = removing.Select(x => x.ToReference<BlueprintArchetypeReference>()).ToList(),
                                            PerLevel = false,
                                            IncreasePerTick = resource.m_MaxAmount.PerStepIncrease,
                                            StartLevel = resource.m_MaxAmount.StartingLevel,
                                            StartIncrease = resource.m_MaxAmount.StartingIncrease,
                                            LevelStep = resource.m_MaxAmount.LevelStep
                                        };
                                    }



                                }
                                else if (classToProbe.Archetypes.Any(x => x.AddFeatures.Any(x => x.Features.Any(x => x.ToReference<BlueprintFeatureBaseReference>().Equals(feature.ToReference<BlueprintFeatureBaseReference>())))))
                                {
                                    var adding = classToProbe.Archetypes.Where(x => x.AddFeatures.Any(x => x.Features.Any(x => x.ToReference<BlueprintFeatureBaseReference>().Equals(feature.ToReference<BlueprintFeatureBaseReference>()))));
                                    classEntry.archetypeEntries.Add(new ArchetypeEntry
                                    {
                                        Archetypes = adding.Select(x => x.ToReference<BlueprintArchetypeReference>()).ToList(),
                                        PerLevel = false,
                                        IncreasePerTick = resource.m_MaxAmount.PerStepIncrease,
                                        StartLevel = resource.m_MaxAmount.StartingLevel,
                                        StartIncrease = resource.m_MaxAmount.StartingIncrease,
                                        LevelStep = resource.m_MaxAmount.LevelStep
                                    });

                                }
                                else if (classToProbe.PrestigeClass)
                                {
                                    classEntry.vanilla = new VanillaEntry
                                    {

                                        PerLevel = false,
                                        IncreasePerTick = resource.m_MaxAmount.PerStepIncrease,
                                        StartLevel = resource.m_MaxAmount.StartingLevel,
                                        StartIncrease = resource.m_MaxAmount.StartingIncrease,
                                        LevelStep = resource.m_MaxAmount.LevelStep
                                    };
                                }
                                else
                                {
                                    NoteFailure($"Could not confirm approriateness of entry for {classToProbe.name} from {feature.Name} on {unification}");
                                }
                            }
                        }



                    }
                   
                }
                if (!resource.m_MaxAmount.IncreasedByLevel && !resource.m_MaxAmount.IncreasedByLevelStartPlusDivStep)
                {
                    NoteFailure($"{feature.Name} is not a level scaled resource adder, it should not have been routed here");
                }


            }
        }

       
        private static Dictionary<BlueprintCharacterClassReference, List<BlueprintArchetypeReference>> ExtractDict(List<BlueprintCharacterClass> classes, List<BlueprintArchetype> arches)
        {
            Dictionary<BlueprintCharacterClassReference, List<BlueprintArchetypeReference>> classToArchMappings = new();
            foreach (BlueprintCharacterClass v in classes)
            {
                if (!classToArchMappings.ContainsKey(v.ToReference<BlueprintCharacterClassReference>()))
                {
                    classToArchMappings.Add(v.ToReference<BlueprintCharacterClassReference>(), new());

                }
            }
            foreach (BlueprintArchetype v in arches)
            {
                if (classToArchMappings.TryGetValue(v.m_ParentClass.ToReference<BlueprintCharacterClassReference>(), out var parentList))
                {
                    parentList.Add(v.ToReference<BlueprintArchetypeReference>());
                }
            }
            return classToArchMappings;
        }




        private static bool AddAltStatToUnification(LevelScalingUnification unification, StatType stat, BlueprintFeatureReference unlock, bool FromClass)
        {
            if (!unification.UsesStat)
            {
                NoteFailure($"Aborting Alt Stat Registration. Base Stat registered but does not use stat scaling. Make a custom feature instead");
                return false;
            }
            var resourceBP = unification.BaseResource.Get();
            var existing = resourceBP.Components.OfType<ResourceSourceInfoComponent>().FirstOrDefault(x => x.m_Unlock.Equals(unlock));
            if (existing != null)
            {
                NoteFailure($"{unlock.NameSafe()} already providing an altStat to {resourceBP.NameSafe()}, skipping redundancy");
                return false;
            }
            else
            {
                resourceBP.AddComponent<ResourceSourceInfoComponent>(x =>
                {
                    x.AltStat = stat;
                    x.m_Unlock = unlock;
                    x.IsClassFeature = FromClass;
                });
            }
            Main.Context.Logger.LogPatch($"Registered {unlock.NameSafe()} as {stat.ToString()} scaling unlock for ", resourceBP);
            return true;
        }



        
        

    }
}
