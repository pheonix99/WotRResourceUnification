using Kingmaker.Blueprints;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Stats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TabletopTweaks.Core.Utilities;
using WotRResourceUnification.NewComponents;

namespace WotRResourceUnification.Content
{
    public static class ModifyTools
    {
        public static List<string> FailureLogs = new();
        private static List<Unification> unifications = new();
        private static void NoteFailure(string failure)
        {
            Main.Context.Logger.LogError(failure);
            FailureLogs.Add(failure);
        }

        #region public wizards

        public static bool RegistrationWizard(string key, BlueprintFeatureReference reference)
        {
            return RegistrationWizardGuts(key, reference);
        }

        public static bool RegistrationWizardForGUID(string key, string guid)
        {
            var BP = BlueprintTools.GetBlueprintReference<BlueprintFeatureReference>(guid);
            if (BP == null)
            {
                NoteFailure($"Unification Base Wizard failed on {guid} for {key}: no BP");
                return false;
            }
            else return RegistrationWizardGuts(key, BP);

        }

        public static bool AltResourceRegistrationWizard(BlueprintFeatureReference originalFeature, BlueprintFeatureReference altFeature)
        {
            var u = unifications.FirstOrDefault(x => x.BaseFeature.Equals(originalFeature));
            if (u == null)
            {
                NoteFailure($"Alt Resource Wizard failed on {altFeature.NameSafe()} - couldn't find reference {originalFeature.NameSafe()}");
                return false;
            }
            else
                return AltResourceRegistrationWizard(u, altFeature);

        }

        public static bool AltResourceRegistrationWizard(string key, BlueprintFeatureReference altFeature)
        {
            var u = unifications.FirstOrDefault(x => x.Name == key);
            if (u == null)
            {
                NoteFailure($"Alt Resource Wizard failed on {altFeature.NameSafe()} - couldn't find reference {key}");
                return false;
            }
            else
                return AltResourceRegistrationWizard(u, altFeature);

        }

        #endregion
        #region private wizard bits
        private static bool RegistrationWizardGuts(string key, BlueprintFeatureReference reference)
        {
            Main.Context.Logger.LogHeader($"Registering {reference.NameSafe()} in unified resource system, key: {key}");
            var exists = unifications.FirstOrDefault(x => x.Name.Equals(key, StringComparison.OrdinalIgnoreCase));
            if (exists != null)
            {
                return AltResourceRegistrationWizard(exists, reference);
            }
            else
            {
                return RegisterBaseResourceWizard(key, reference);
            }

        }
        private static bool RegisterBaseResourceWizard(string key, BlueprintFeatureReference reference)
        {
            var resourceBp = reference?.Get();
            var addResource = resourceBp.Components.OfType<AddAbilityResources>().ToList();
            bool defaultIncreasedByStat = false;
            if (addResource.Count() != 1)
            {
                NoteFailure($"Unification Base Wizard failed on {reference.NameSafe()}: wrong AddAbilityResource count");
                return false;
            }
            if (addResource[0].m_Resource.Get().m_MaxAmount.IncreasedByStat)
            {
                defaultIncreasedByStat = true;
            }

            return RegisterBaseResource(key, reference, addResource[0].m_Resource, defaultIncreasedByStat, addResource[0].m_Resource.Get().m_MaxAmount.ResourceBonusStat);


        }

        private static bool AltResourceRegistrationWizard(Unification u, BlueprintFeatureReference altFeature)
        {
            var resourceBp = altFeature?.Get();
            var addResource = resourceBp.Components.OfType<AddAbilityResources>().ToList();

            if (addResource.Count() != 1)
            {
                NoteFailure($"Alt Resource Wizard failed on {altFeature.NameSafe()}: wrong AddAbilityResource count");
                return false;
            }


            return RegisterAltResource(u, addResource[0].m_Resource, addResource[0].m_Resource.Get().m_MaxAmount.ResourceBonusStat, altFeature);
        }

        #endregion







        private static bool RegisterBaseResource(string key, BlueprintFeatureReference baseAdderFeature, BlueprintAbilityResourceReference baseResourceRef, bool usesStat, StatType statType = StatType.Unknown)
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
                var unification = new Unification(key, baseAdderFeature, baseResourceRef, usesStat, statType);
                unifications.Add(unification);
                var baseResource = baseResourceRef.Get();
                if (usesStat)
                {
                    baseResource.AddComponent<AltAbilityBonusStatUnlockComponent>(x =>
                    {
                        x.AltStat = statType;
                        x.m_Unlock = baseAdderFeature;
                    });
                    Main.Context.Logger.LogPatch($"Registered {baseAdderFeature.NameSafe()} as {statType.ToString()} scaling unlock for ", baseResource);
                }

                //unification.BaseAmounts.Add(new Tuple<BlueprintAbilityResource.Amount, BlueprintFeatureReference>(Copy(baseResource.m_MaxAmount), baseAdderFeature));


                return true;
            }
        }

        private static BlueprintAbilityResource.Amount Copy(BlueprintAbilityResource.Amount original)
        {

            return new BlueprintAbilityResource.Amount
            {
                StartingLevel = original.StartingLevel,
                StartingIncrease = original.StartingIncrease,
                BaseValue = original.BaseValue,
                m_Archetypes = (BlueprintArchetypeReference[])original.m_Archetypes.Where(x => x != null).ToArray(),
                m_ArchetypesDiv = (BlueprintArchetypeReference[])original.m_ArchetypesDiv.Where(x => x != null).ToArray(),
                IncreasedByLevel = original.IncreasedByLevel,
                IncreasedByLevelStartPlusDivStep = original.IncreasedByLevelStartPlusDivStep,
                LevelIncrease = original.LevelIncrease,
                IncreasedByStat = original.IncreasedByStat,
                LevelStep = original.LevelStep,
                MinClassLevelIncrease = original.MinClassLevelIncrease,
                m_Class = original.m_Class.Where(x => x != null).ToArray(),
                m_ClassDiv = original.m_ClassDiv.Where(x => x != null).ToArray(),
                OtherClassesModifier = original.OtherClassesModifier,
                PerStepIncrease = original.PerStepIncrease,
                ResourceBonusStat = original.ResourceBonusStat

            };
        }




        private static bool RegisterAltResource(Unification u, BlueprintAbilityResourceReference redirectFrom, StatType statType, BlueprintFeatureReference unlock)
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
                altStatted = AddAltStatToUnification(u, statType, unlock);
            }
            return redirected || altStatted;
        }

        private static bool RegisterResourceRedirect(Unification unifiedResource, BlueprintFeatureReference unlock, BlueprintAbilityResourceReference redirectFrom)
        {
            var fromBP = redirectFrom.Get();
            var existingRedirect = fromBP.Components.OfType<ResourceUnificationRedirectComponent>().FirstOrDefault();
            if (existingRedirect != null)
            {
                if (existingRedirect.m_RedirectTo.Equals(unifiedResource.BaseResource))
                {
                    Main.Context.Logger.Log($"{fromBP.Name} already redirects to {existingRedirect.m_RedirectTo.Get().name}, skipping redundancy");
                }
                else
                {
                    Main.Context.Logger.Log($"{fromBP.Name} already redirects to {existingRedirect.m_RedirectTo.Get().name}, redirect conflict detected!");
                }
                return false;
            }
            else
            {
                fromBP.AddComponent<ResourceUnificationRedirectComponent>(x =>
                {
                    x.m_RedirectTo = unifiedResource.BaseResource;
                });
                Main.Context.Logger.LogPatch($"Added resource redirect to {unifiedResource.BaseResource.NameSafe()} from:", fromBP);
                var baseResource = unifiedResource.BaseResource.Get();
                var altResource = redirectFrom.Get();

                //unifiedResource.BaseAmounts.Add(new Tuple<BlueprintAbilityResource.Amount, BlueprintFeatureReference>(Copy(baseResource.m_MaxAmount), unlock));


                unifiedResource.UnifiedResources.Add(redirectFrom);
                //Main.Context.Logger.LogPatch($"Unification with {unifiedResource.Name} Applied to", fromBP);
                return true;
            }


        }

        private static ExtendedAmount ProcessBlueprintAbilityResource(BlueprintAbilityResource blueprintAbilityResource)
        {
            if (blueprintAbilityResource.m_MaxAmount.IncreasedByLevel)
            {
                var classesWithoutArchetypes = blueprintAbilityResource.m_MaxAmount.Class.Where(x => x.Archetypes.Any(y => blueprintAbilityResource.m_MaxAmount.Archetypes.Contains(y)));
                foreach(var charclass in classesWithoutArchetypes)
                {
                    if (charclass.Progression.LevelEntries.Any(x=>x.Features.Any(y=>y.GetComponent<AddAbilityResources>(x=>x.Resource.Equals(blueprintAbilityResource)))))
                    {

                    }
                }
            }
            return null;


        }

        private static void DoClassScalingResoulution(Unification unification)
        {


            for(int i = 0; i <unification.BaseAmounts.Count(); i++)
            {

            }
        }

        private static bool AddAltStatToUnification(Unification unification, StatType stat, BlueprintFeatureReference unlock)
        {
            if (!unification.UsesStat)
            {
                NoteFailure($"Aborting Alt Stat Registration. Base Stat registered but does not use stat scaling. Make a custom feature instead");
                return false;
            }
            var resourceBP = unification.BaseResource.Get();
            var existing = resourceBP.Components.OfType<AltAbilityBonusStatUnlockComponent>().FirstOrDefault(x => x.m_Unlock.Equals(unlock));
            if (existing != null)
            {
                NoteFailure($"{unlock.NameSafe()} already providing an altStat to {resourceBP.NameSafe()}, skipping redundancy");
                return false;
            }
            else
            {
                resourceBP.AddComponent<AltAbilityBonusStatUnlockComponent>(x =>
                {
                    x.AltStat = stat;
                    x.m_Unlock = unlock;

                });
            }
            Main.Context.Logger.LogPatch($"Registered {unlock.NameSafe()} as {stat.ToString()} scaling unlock for ", resourceBP);
            return true;
        }

        

        public static bool RegisterAltStat(BlueprintAbilityResourceReference resource, StatType stat, BlueprintFeatureReference unlock)
        {
            var unification = unifications.FirstOrDefault(x => x.BaseResource.Equals(resource));
            if (unification == null)
            {
                NoteFailure($"Aborting Alt Stat Registration. Base Stat not configured for: {resource.NameSafe()} - alt-stat would break base class");
                return false;
            }
            else
            {
                return AddAltStatToUnification(unification, stat, unlock);
            }
            

            
        }

    }
}
