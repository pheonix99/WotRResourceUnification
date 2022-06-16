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

namespace ResourceUnification.Content
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
                    foreach(var v in Main.Context.ResourceDefines.ClassScalingResourceEntries)
                    {
                        
                            foreach(string s in v.ClassResourceFeatureGuids)
                            {
                                ModifyTools.RegistrationWizardForGUID(v.Key, s, true);
                            }
                                foreach(string s in v.NonClassResourceFeatureGuids)
                            {
                                ModifyTools.RegistrationWizardForGUID(v.Key, s, false);
                            }


                    }




                    ModifyTools.Finish();


                }
                catch (Exception e)
                {

                    Main.Context.Logger.LogError(e, $"Error caught in Late patch");
                }
            }
        }


        
    }
}
