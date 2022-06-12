using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TabletopTweaks.Core.Utilities;

namespace WotRResourceUnification.Content
{
    class Unifications
    {
        

        [HarmonyPatch(typeof(BlueprintsCache), "Init")]
        static class BlueprintsCache_Init_Patch2
        {
            static bool Initialized;

            [HarmonyPriority(Priority.Last)]
            static void Postfix()
            {
                try
                {
                    Main.Context.Logger.LogHeader($"Doing Built In Unifications");
                    //ScaledFist();
                    foreach(var v in Main.Context.ResourceDefines.ResourceEntries)
                    {
                        bool mainSucceded = ModifyTools.RegistrationWizardForGUID(v.Key, v.BaseResourceFeatureGuid);
                        if (mainSucceded)
                        {
                            foreach(string s in v.AltResourceFeatureGuids)
                            {
                                ModifyTools.RegistrationWizardForGUID(v.Key, s);
                            }
                        }


                    }







                }
                catch (Exception e)
                {

                    Main.Context.Logger.LogError(e, $"Error caught in Late patch");
                }
            }
        }


        private static void ScaledFist()
        {
            var KiPower = BlueprintTools.GetBlueprint<BlueprintAbilityResource>("9d9c90a9a1f52d04799294bf91c80a82");
            var ScaledFistPower = BlueprintTools.GetBlueprint<BlueprintAbilityResource>("7d002c1025fbfe2458f1509bf7a89ce1");
            var KiPowerFeature = BlueprintTools.GetBlueprint<BlueprintFeature>("e9590244effb4be4f830b1e3fffced13");
            var ScaledFistKiPowerFeature = BlueprintTools.GetBlueprint<BlueprintFeature>("ae98ab7bda409ef4bb39149a212d6732");

            ModifyTools.RegistrationWizard("Ki", KiPowerFeature.ToReference<BlueprintFeatureReference>());

            ModifyTools.RegistrationWizard("Ki", ScaledFistKiPowerFeature.ToReference<BlueprintFeatureReference>());
            ModifyTools.RegistrationWizardForGUID("Ki", "5661f68399d77ba48bee60df871d7728");

            //ModifyTools.RegisterAltStat(KiPower.ToReference<BlueprintAbilityResourceReference>(), Kingmaker.EntitySystem.Stats.StatType.Wisdom, KiPowerFeature.ToReference<BlueprintFeatureReference>());
            //ModifyTools.RegisterAltStat(KiPower.ToReference<BlueprintAbilityResourceReference>(), Kingmaker.EntitySystem.Stats.StatType.Charisma, ScaledFistKiPowerFeature.ToReference<BlueprintFeatureReference>());

            //ModifyTools.RegisterResourceRedirect(ScaledFistPower.ToReference<BlueprintAbilityResourceReference>(), KiPower.ToReference<BlueprintAbilityResourceReference>());
        }
    }
}
