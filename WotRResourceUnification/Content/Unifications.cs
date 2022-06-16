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

                    ModifyTools.RegisterForProcessingAsBaseGameAnchor("Ki", "e9590244effb4be4f830b1e3fffced13");
                    ModifyTools.RegisterForProcessingAsBaseGameAnchor("ArcanePool", "3ce9bb90749c21249adc639031d5eed1");
                    
                    ModifyTools.RegisterForProcessing("Ki", "ae98ab7bda409ef4bb39149a212d6732", true);
                    ModifyTools.RegisterForProcessing("ArcanePool", "95e04a9e86aa9e64dad7122625b79c62", true);
                    //ScaledFist();
                    foreach (var v in Main.Context.ResourceDefines.ClassScalingResourceEntries)
                    {
                        
                            foreach(string s in v.ClassResourceFeatureGuids)
                            {
                                ModifyTools.RegisterForProcessing(v.Key, s, true);
                            }
                                foreach(string s in v.NonClassResourceFeatureGuids)
                            {
                                ModifyTools.RegisterForProcessing(v.Key, s, false);
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
