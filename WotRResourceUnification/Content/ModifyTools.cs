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
using ResourceUnification.Config;
using Kingmaker.Utility;
using Kingmaker;

namespace ResourceUnification.Content
{
    public static class ModifyTools
    {
        public static List<string> FailureLogs = new();
        private static List<LevelScalingUnification> unifications = new();
        public static List<LevelScalingUnification> Unifications => unifications.ToList();

        private static void NoteFailure(string failure)
        {
            Main.Context.Logger.LogError(failure);
            FailureLogs.Add(failure);
        }

        #region public wizards

        public static void RegisterForProcessing(string key, BlueprintFeatureReference reference, bool fromClass)
        {
            RegisterForProcessing(new ResourceFeatureInfo(key, reference, fromClass));
        }

       

       public static void RegisterForProcessing(string key, string guid, bool fromClass)
        {
            var BP = BlueprintTools.GetBlueprint<BlueprintFeature>(guid)?.ToReference<BlueprintFeatureReference>();
            if (BP == null)
            {
                NoteFailure($"Unification Base Wizard failed on {guid} for {key}: no BP");
                return;
            }
            RegisterForProcessing(new ResourceFeatureInfo(key, BP, fromClass));

        }

        public static void RegisterForProcessing(string key, GameResourceEntry gre, bool fromClass)
        {
            var BP = BlueprintTools.GetBlueprint<BlueprintFeature>(gre.ResourceAdderFeatureGuid);
            if (BP == null)
            {
                NoteFailure($"Unification Base Wizard failed on {gre.ResourceAdderFeatureGuid} for {key}: no BP");
                return;
            }
            BlueprintFeatureReference altBP = null;
            if (!string.IsNullOrWhiteSpace(gre.WrapperGuid))
            {
                 altBP = BlueprintTools.GetBlueprint<BlueprintFeature>(gre.WrapperGuid)?.ToReference<BlueprintFeatureReference>();
            }
            
            RegisterForProcessing(new ResourceFeatureInfo(key, BP.ToReference<BlueprintFeatureReference>(), fromClass, altBP));

        }

        internal static void Finish()
        {
            int preprocess = 0;
            do
            {
                var good = UnificationPreproccess(unifications[preprocess]);
                if (good)
                {
                    preprocess++;
                }
                else
                {
                    unifications.RemoveAt(preprocess);
                }
            } while (preprocess < unifications.Count());


            int progress = 0;
            do
            {
                BuildUnification(unifications[progress]);
                progress++;
            } while (progress < unifications.Count());
        }

        private static void RegisterForProcessing(ResourceFeatureInfo resourceFeatureInfo)
        {
            var unification = unifications.FirstOrDefault(x => x.Name.Equals(resourceFeatureInfo.Key, StringComparison.OrdinalIgnoreCase));


            if (unification == null)
            {
                unification = new LevelScalingUnification(resourceFeatureInfo.Key);
                unifications.Add(unification);
            }
            if (!unification.UnprocessedResourceAddingFeatures.Any(x => x.ResourceHoldingFeature.deserializedGuid.Equals(resourceFeatureInfo.ResourceHoldingFeature.deserializedGuid) && x.ProgressionFacingFeature.deserializedGuid.Equals(resourceFeatureInfo.ProgressionFacingFeature.deserializedGuid)))
            {
                unification.UnprocessedResourceAddingFeatures.Add(resourceFeatureInfo);
#if DEBUG
                Main.Context.Logger.Log($"Registered {resourceFeatureInfo.ResourceHoldingFeature.NameSafe()} for processing");
#endif
            }
            else
            {
#if DEBUG
                Main.Context.Logger.Log($"Did Not Register {resourceFeatureInfo.ResourceHoldingFeature.NameSafe()} for processing - redundancy");
#endif
            }    
            

        }
        internal static void RegisterForProcessingAsBaseGameAnchor(string key, string guid)
        {
            RegisterForProcessingAsBaseGameAnchor(key, new GameResourceEntry { ResourceAdderFeatureGuid = guid });
        }

        internal static void RegisterForProcessingAsBaseGameAnchor(string key, GameResourceEntry gre)
        {
            var BP = BlueprintTools.GetBlueprintReference<BlueprintFeatureReference>(gre.ResourceAdderFeatureGuid);
            BlueprintFeatureReference altBP = null;
            if (!string.IsNullOrWhiteSpace(gre.WrapperGuid))
            {
                altBP = BlueprintTools.GetBlueprintReference<BlueprintFeatureReference>(gre.WrapperGuid);
            }
            if (BP == null)
            {
                NoteFailure($"Unification Base Wizard failed on {gre.ResourceAdderFeatureGuid} for {key}: no BP");
                return;
            }
            
            ResourceFeatureInfo resourceFeatureInfo = new ResourceFeatureInfo(key, BP, true, altBP);
            var unification = unifications.FirstOrDefault(x => x.Name.Equals(resourceFeatureInfo.Key, StringComparison.OrdinalIgnoreCase));


            if (unification == null)
            {
                unification = new LevelScalingUnification(resourceFeatureInfo.Key);
                unifications.Add(unification);
                unification.UnprocessedResourceAddingFeatures.Add(resourceFeatureInfo);
            }
            else
            {
                unification.UnprocessedResourceAddingFeatures.Insert(0, resourceFeatureInfo);
            }
#if DEBUG
            Main.Context.Logger.Log($"Registed {resourceFeatureInfo.ResourceHoldingFeature.NameSafe()} for processing as hardcoded anchor");
#endif


        }

        #endregion
        #region private wizard bits



        #endregion






        /*
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
        */


        /*

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
            if (u.StatUnlocks.ContainsKey(unlock))
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
                unifiedResource.ProcessedResourceFeatures.Add(unlock);
                //Main.Context.Logger.LogPatch($"Unification with {unifiedResource.Name} Applied to", fromBP);
                return true;
            }


        }
        */
        public static string MakeArcheLog (IEnumerable<BlueprintArchetypeReference> list)
        {
            string s = "";
            var listT = list.ToArray();
            for(int i = 0; i< listT.Count(); i++)
            {
                s += listT[i].NameSafe();
                if (i + 1 < listT.Length)
                    s += ", ";
            }
            

            return s;
        }

        private static bool UnificationPreproccess(LevelScalingUnification unification)
        {
            int checkpoint = 0;
            do
            {
                var entry = unification.UnprocessedResourceAddingFeatures[checkpoint];
                var feature = entry.ResourceHoldingFeature.Get();
                if (feature.Components.OfType<AddAbilityResources>().Any())
                {
                    entry.Resource = feature.Components.OfType<AddAbilityResources>().FirstOrDefault().m_Resource;
                    entry.Amount = entry.Resource.Get().m_MaxAmount;
                    if (unification.BaseInfo == null)
                    {
                        unification.BaseInfo = entry;
                    }
                    if (!unification.ResourcesCovered.Contains(entry.Resource))
                    {
                        unification.ResourcesCovered.Add(entry.Resource);
                    }
                    checkpoint++;
                }
                else
                {
                    unification.UnprocessedResourceAddingFeatures.Remove(entry);
                }


            } while (checkpoint < unification.UnprocessedResourceAddingFeatures.Count);
            if (!unification.UnprocessedResourceAddingFeatures.Any())
                return false;
            if (unification.BaseInfo == null)
            {
                return false;
            }
            return true;
        }

        public static void ScanAllClasses()
        {
            var root = Game.Instance.BlueprintRoot;
            var resources = unifications.SelectMany(x => x.ResourcesCovered);
            

        }

        public static void BuildUnification(LevelScalingUnification unification)
        {
            
#if DEBUG
            Main.Context.Logger.Log($"Doing full resource unification for {unification.Name}");
#endif
            




            var baseResource = unification.BaseInfo.Resource.Get();



            var existing = baseResource.Components.OfType<ImprovedAbilityResourceCalc>().FirstOrDefault();
            if (existing == null)
            {
                baseResource.AddComponent<ImprovedAbilityResourceCalc>(x =>
                {
                    
                    x.UsesStat = unification.BaseInfo.Amount.IncreasedByStat;
                    x.OtherClassesModifier = unification.BaseInfo.Amount.OtherClassesModifier;


                });
            }



            var extendedAmount = baseResource.Components.OfType<ImprovedAbilityResourceCalc>().FirstOrDefault();

            do
            {
               

                var info = unification.UnprocessedResourceAddingFeatures[0];
                var feature = info.ResourceHoldingFeature.Get();
                var progFeature = info.ProgressionFacingFeature.Get();
                Main.Context.Logger.Log($"Adding {feature.name} to {unification.Name}");
                var resource = info.Resource.Get();
                var amount = info.Amount;
                if (resource == null)
                {
                    Main.Context.Logger.Log($"Cannot find AddAbilityResources for {feature.Name}, skipping");
                    continue;
                }
                if (info != unification.BaseInfo && !resource.Components.OfType<ResourceRedirectComponent>().Any())
                {
                    resource.AddComponent<ResourceRedirectComponent>(x =>
                    {
                        x.m_RedirectTo = unification.BaseInfo.Resource;

                    });
#if DEBUG
                    Main.Context.Logger.Log($"Build Redirect from {resource.Name} to {unification.BaseInfo.Resource.NameSafe()}");
#endif
                }
                if (amount.IncreasedByStat)
                {
                    baseResource.AddComponent<ResourceSourceInfoComponent>(x =>
                   {
                       x.AltStat = amount.ResourceBonusStat;
                       x.IsClassFeature = info.FromClass;
                       x.m_Unlock = info.ResourceHoldingFeature;

                   });
                }
                if (amount.IncreasedByStat)
                {
                    extendedAmount.UsesStat = true;
                }

                Main.Context.Logger.Log($"Calculating class by class progression for {feature.name}");

                if (amount.IncreasedByLevel)
                {
                    var maps = ExtractDict(amount.m_Class.Where(x => x != null).Select(x => x.Get()).ToList(), amount.m_Archetypes.Where(x => x != null).Select(x => x.Get()).ToList());
                    if (maps.Count == 0 && amount.OtherClassesModifier == 0f)
                    {
                        NoteFailure($"{feature.Name} has IncreasedByLevel set but no classes set for that - this could be a blueprint error or a thing I don't understand yet");

                    }
                    else
                    {
                        foreach (var charClass in maps)
                        {
#if DEBUG
                            Main.Context.Logger.Log($"Building class entry for {charClass.Key.NameSafe()} on {unification.Name}");
#endif
                            ClassEntry classEntry;
                            if (!extendedAmount.classEntries.TryGetValue(charClass.Key, out classEntry))
                            {
#if DEBUG
                                Main.Context.Logger.Log($"Built classEntry for {unification.Name} : {charClass.Key.NameSafe()}");
#endif
                                classEntry = new ClassEntry(charClass.Key);
                                extendedAmount.classEntries.Add(charClass.Key, classEntry);
                            }


                            if (charClass.Value.Any()) //Base Archetypes
                            {
                                var newSubentry= new ArchetypeEntry
                                {
                                    Archetypes = charClass.Value

                                };
                                ModifySubclassGainLevel(newSubentry, amount);
                                classEntry.archetypeEntries.Add(newSubentry);
#if DEBUG
                                Main.Context.Logger.Log($"Built archetype classEntry for {unification.Name}  {charClass.Key.NameSafe()}: {  MakeArcheLog(charClass.Value)}");
#endif
                            }
                            else
                            {
                                var classToProbe = charClass.Key.Get();
                                var levels = classToProbe.Progression.LevelEntries.Where(x => x.Features.Any(x => x.ToReference<BlueprintFeatureBaseReference>().Equals(progFeature.ToReference<BlueprintFeatureBaseReference>()))).Select(x => x.Level);
                                if (classToProbe.Progression.LevelEntries.Any(x => x.Features.Any(x => x.ToReference<BlueprintFeatureBaseReference>().Equals(progFeature.ToReference<BlueprintFeatureBaseReference>()))))
                                {

                                    var removing = classToProbe.Archetypes.Where(x => x.RemoveFeatures.Any(x => x.Features.Any(x => x.ToReference<BlueprintFeatureBaseReference>().Equals(progFeature.ToReference<BlueprintFeatureBaseReference>()))));//get arches that remove feature
                                    removing = removing.Where(x => x.RemoveFeatures.Where(x => x.Features.Any(x => x.ToReference<BlueprintFeatureBaseReference>().Equals(progFeature.ToReference<BlueprintFeatureBaseReference>()))).Select(x => x.Level).SequenceEqual(levels));//cut out ones that don't remove all instances
                                    
                                   
                                    //removing = removing.Where(x => !x.AddFeatures.FirstOrDefault(x => x.Level == levels.FirstOrDefault())?.Features.Any(x => x.Components.OfType<AddAbilityResources>().FirstOrDefault()?.Resource.Equals(resource) == true) == true);
                                    if (classEntry.vanilla != null)
                                    {
                                        NoteFailure($"Found extra non-archetype entries for {classToProbe.name} from {progFeature.Name} on {unification.Name}");
                                    }
                                    else
                                    {
                                        var newSubentry = new VanillaEntry
                                        {
                                            BlockedArchetypes = removing.Select(x => x.ToReference<BlueprintArchetypeReference>()).ToList()

                                        };
                                        ModifySubclassGainLevel(newSubentry, amount);
                                        classEntry.vanilla = newSubentry;

                                    }
#if DEBUG
                                    Main.Context.Logger.Log($"Built vanilla classEntry for {unification.Name}  {charClass.Key.NameSafe()} with blocked archetypes {MakeArcheLog(removing.Select(x => x.ToReference<BlueprintArchetypeReference>()).ToList())}");
#endif


                                }
                                else if (classToProbe.Archetypes.Any(x => x.AddFeatures.Any(x => x.Features.Any(x => x.ToReference<BlueprintFeatureBaseReference>().Equals(progFeature.ToReference<BlueprintFeatureBaseReference>())))))
                                {
                                    var adding = classToProbe.Archetypes.Where(x => x.AddFeatures.Any(x => x.Features.Any(x => x.ToReference<BlueprintFeatureBaseReference>().Equals(progFeature.ToReference<BlueprintFeatureBaseReference>()))));
                                    var newSubentry = new ArchetypeEntry
                                    {
                                        Archetypes = adding.Select(x => x.ToReference<BlueprintArchetypeReference>()).ToList(),

                                    };
                                    ModifySubclassGainLevel(newSubentry, amount);
                                    classEntry.archetypeEntries.Add(newSubentry);
                                    Main.Context.Logger.Log($"Built archetype classEntry for {unification.Name}  {charClass.Key.NameSafe()} with added archetypes {MakeArcheLog(adding.Select(x => x.ToReference<BlueprintArchetypeReference>()).ToList())}");
                                }
                                else if (classToProbe.PrestigeClass)
                                {
                                    var newSubentry = new VanillaEntry
                                    {
                                        

                                    };
                                    ModifySubclassGainLevel(newSubentry, amount);
                                    classEntry.vanilla = newSubentry;
                                }
                                else
                                {
                                    NoteFailure($"Could not confirm approriateness of entry for {classToProbe.name} from {feature.name} with prog facing feature {progFeature.name} on {unification.Name}");
                                }
                            }
                        }



                    }



                }
                if (amount.IncreasedByLevelStartPlusDivStep)
                {
                    var maps = ExtractDict(amount.m_ClassDiv.Where(x => x != null).Select(x => x.Get()).ToList(), amount.m_Archetypes.Where(x => x != null).Select(x => x.Get()).ToList());
                    if (maps.Count == 0 && amount.OtherClassesModifier == 0f)
                    {
                        NoteFailure($"{feature.Name} has IncreasedByLevel set but no classes set for that - this could be a blueprint error or a thing I don't understand yet");

                    }
                    else
                    {
                        foreach (var charClass in maps)
                        {
#if DEBUG
                            Main.Context.Logger.Log($"Building class entry for {charClass.Key.NameSafe()} on {unification.Name}");
#endif
                            ClassEntry classEntry;
                            if (!extendedAmount.classEntries.TryGetValue(charClass.Key, out classEntry))
                            {
#if DEBUG
                                Main.Context.Logger.Log($"Built classEntry for {unification.Name} : {charClass.Key.NameSafe()}");
#endif
                                classEntry = new ClassEntry(charClass.Key);
                                extendedAmount.classEntries.Add(charClass.Key, classEntry);

                            }


                            if (charClass.Value.Any())
                            {

                                classEntry.archetypeEntries.Add(new ArchetypeEntry
                                {
                                    
                                    IncreasePerTick = resource.m_MaxAmount.PerStepIncrease,
                                    StartLevel = resource.m_MaxAmount.StartingLevel,
                                    StartIncrease = resource.m_MaxAmount.StartingIncrease,
                                    LevelStep = resource.m_MaxAmount.LevelStep,
                                    PerLevel = false

                                });
                                var newSubentry = new ArchetypeEntry
                                {
                                    Archetypes = charClass.Value

                                };
                                ModifySubclassGainLevelWithDiv(newSubentry, amount);
                                classEntry.archetypeEntries.Add(newSubentry);

#if DEBUG
                                Main.Context.Logger.Log($"Built archetype classEntry for {unification.Name} and added {charClass.Key.NameSafe()}: {MakeArcheLog(charClass.Value)}");
#endif

                            }
                            else
                            {
                                var classToProbe = charClass.Key.Get();
                                var levels = classToProbe.Progression.LevelEntries.Where(x => x.Features.Any(x => x.ToReference<BlueprintFeatureBaseReference>().Equals(progFeature.ToReference<BlueprintFeatureBaseReference>()))).Select(x => x.Level);
                                if (classToProbe.Progression.LevelEntries.Any(x => x.Features.Any(x => x.ToReference<BlueprintFeatureBaseReference>().Equals(progFeature.ToReference<BlueprintFeatureBaseReference>()))))
                                {

                                    //var removing = classToProbe.Archetypes.Where(x => x.RemoveFeatures.Any(x => x.Features.Any(x => x.ToReference<BlueprintFeatureBaseReference>().Equals(progFeature.ToReference<BlueprintFeatureBaseReference>()))));
                                    var removing = classToProbe.Archetypes.Where(x => x.RemoveFeatures.Any(x => x.Features.Any(x => x.ToReference<BlueprintFeatureBaseReference>().Equals(progFeature.ToReference<BlueprintFeatureBaseReference>()))));//get arches that remove feature
                                    removing = removing.Where(x => x.RemoveFeatures.Where(x => x.Features.Any(x => x.ToReference<BlueprintFeatureBaseReference>().Equals(progFeature.ToReference<BlueprintFeatureBaseReference>()))).Select(x => x.Level).SequenceEqual(levels));//cut out ones that don't remove all instances
                                    
                                    //removing = removing.Where(x => !x.AddFeatures.FirstOrDefault(x => x.Level == levels.FirstOrDefault())?.Features.Any(x => x.Components.OfType<AddAbilityResources>().FirstOrDefault()?.Resource.Equals(resource) == true) == true);
                                    if (classEntry.vanilla != null)
                                    {
                                        NoteFailure($"Found extra non-archetype entries for {classToProbe.name} from {progFeature.Name} on {unification}");
                                    }
                                    else
                                    {
                                        classEntry.vanilla = new VanillaEntry
                                        {
                                            BlockedArchetypes = removing.Select(x => x.ToReference<BlueprintArchetypeReference>()).ToList(),
                                        };
                                        ModifySubclassGainLevelWithDiv(classEntry.vanilla, amount);

                                        
                                    }
#if DEBUG
                                    Main.Context.Logger.Log($"Built vanilla classEntry for {unification.Name}  {charClass.Key.NameSafe()} with blocked archetypes {MakeArcheLog(removing.Select(x=>x.ToReference<BlueprintArchetypeReference>()))}");
#endif


                                }
                                else if (classToProbe.Archetypes.Any(x => x.AddFeatures.Any(x => x.Features.Any(x => x.ToReference<BlueprintFeatureBaseReference>().Equals(progFeature.ToReference<BlueprintFeatureBaseReference>())))))
                                {
                                    var adding = classToProbe.Archetypes.Where(x => x.AddFeatures.Any(x => x.Features.Any(x => x.ToReference<BlueprintFeatureBaseReference>().Equals(progFeature.ToReference<BlueprintFeatureBaseReference>()))));
                                    
                                    var newSubentry = new ArchetypeEntry
                                    {
                                        Archetypes = adding.Select(x => x.ToReference<BlueprintArchetypeReference>()).ToList()

                                    };
                                    ModifySubclassGainLevelWithDiv(newSubentry, amount);
                                    classEntry.archetypeEntries.Add(newSubentry);
                                    Main.Context.Logger.Log($"Built archetype classEntry for {unification.Name}  {charClass.Key.NameSafe()} with added archetypes {MakeArcheLog(adding.Select(x => x.ToReference<BlueprintArchetypeReference>()).ToList())}");
                                }
                                else if (classToProbe.PrestigeClass)
                                {
                                    classEntry.vanilla = new VanillaEntry();
                                    ModifySubclassGainLevelWithDiv(classEntry.vanilla, amount);
                                    
                                }
                                else
                                {
                                    NoteFailure($"Could not confirm approriateness of entry for {classToProbe.name} from {feature.name} with prog facing feature {progFeature.name} on {unification.Name}");
                                }
                            }
                        }



                    }

                }
                if (!amount.IncreasedByLevel && !amount.IncreasedByLevelStartPlusDivStep)
                {
                    NoteFailure($"{feature.Name} is not a level scaled resource adder, it should not have been routed here");
                }

                unification.ProcessedResourceFeatures.Add(info);
                unification.UnprocessedResourceAddingFeatures.Remove(info);
            } while (unification.UnprocessedResourceAddingFeatures.Any());

        }

        private static void ModifySubclassGainLevel(ClassGainSubEntry classGainSubEntry, BlueprintAbilityResource.Amount amount)
        {
            
                classGainSubEntry.PerLevel = true;
                classGainSubEntry.IncreasePerTick = amount.LevelIncrease;
            classGainSubEntry.BaseValue = amount.BaseValue;
            
        }
        private static void ModifySubclassGainLevelWithDiv(ClassGainSubEntry classGainSubEntry, BlueprintAbilityResource.Amount amount)
        {
            classGainSubEntry.PerLevel = false;
            classGainSubEntry.IncreasePerTick = amount.PerStepIncrease;
            classGainSubEntry.StartLevel = amount.StartingLevel;
            classGainSubEntry.MinClassLevelIncrease = amount.MinClassLevelIncrease;
            classGainSubEntry.StartIncrease = amount.StartingIncrease;
            classGainSubEntry.BaseValue = amount.BaseValue;
            classGainSubEntry.LevelStep = amount.LevelStep;
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



        /*
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
        */





    }
}
