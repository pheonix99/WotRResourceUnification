using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using ResourceUnification.Config;
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

                    var cavalierResource = BlueprintTools.GetBlueprint<BlueprintAbilityResource>("672e8c9c98db1df4aa66676a66036e71");
                    if (cavalierResource.m_MaxAmount.StartingLevel == 4 && cavalierResource.m_MaxAmount.StartingIncrease == 1)
                    {
                        cavalierResource.m_MaxAmount.StartingLevel = 1;
                        cavalierResource.m_MaxAmount.StartingIncrease = 0;
                    }

                    Main.Context.Logger.LogHeader($"Doing Built In Unifications");

                    ModifyTools.RegisterForProcessingAsBaseGameAnchor("Ki", "e9590244effb4be4f830b1e3fffced13");
                    ModifyTools.RegisterForProcessingAsBaseGameAnchor("ArcanePool", "3ce9bb90749c21249adc639031d5eed1");
                    ModifyTools.RegisterForProcessingAsBaseGameAnchor("ArcanistArcaneReservoir", "55db1859bd72fd04f9bd3fe1f10e4cbb");
                    ModifyTools.RegisterForProcessingAsBaseGameAnchor("AlchemistBomb", new GameResourceEntry { ResourceAdderFeatureGuid = "28384b1d7e25c8743b8bbfc56211ac8c", WrapperGuid = "c59b2f256f5a70a4d896568658315b7d" });
                    //ModifyTools.RegisterForProcessingAsBaseGameAnchor("BardSong", "b92bfc201c6a79e49afd0b5cfbfc269f");

                    ModifyTools.RegisterForProcessing("Ki", "ae98ab7bda409ef4bb39149a212d6732", true);
                    ModifyTools.RegisterForProcessing("ArcanePool", "95e04a9e86aa9e64dad7122625b79c62", true);//EScion
                    ModifyTools.RegisterForProcessing("ArcanePool", "466c40aba50096341bf6532b1e53e8bd", true);//Armored Battlemage
                    ModifyTools.RegisterForProcessing("ArcanePool", new GameResourceEntry { ResourceAdderFeatureGuid = "3ce9bb90749c21249adc639031d5eed1", WrapperGuid = "dea27ccfda4549f4bcc7ac993e83f4b9" }, true);//TTT Bladebound
                    //ModifyTools.RegisterForProcessing("ArcanePool", "dea27ccfda4549f4bcc7ac993e83f4b9" , true);
                    
                    ModifyTools.RegisterForProcessing("ArcanistArcaneReservoir", "9d1e2212594cf47438fff2fa3477b954", true);
                    //ModifyTools.RegisterForProcessing("ArcanistArcaneReservoir", new GameResourceEntry { ResourceAdderFeatureGuid = "55db1859bd72fd04f9bd3fe1f10e4cbb", WrapperGuid = "9d1e2212594cf47438fff2fa3477b954" }, true);
                    

                    //ModifyTools.RegisterForProcessingAsBaseGameAnchor("CavalierChallenge", "dc77cd2ad52cb0e43bb88b264d7af648");

                    //ScaledFist();
                    foreach (var v in Main.Context.ResourceDefines.ClassScalingResourceEntries)
                    {
                        
                            foreach(var classFeature in v.ClassResourceFeatureGuids)
                            {
                                ModifyTools.RegisterForProcessing(v.Key, classFeature, true);
                            }
                                foreach(var nonClassFeature in v.NonClassResourceFeatureGuids)
                            {
                                ModifyTools.RegisterForProcessing(v.Key, nonClassFeature, false);
                            }


                    }
                    foreach(var mod in Main.Context.OtherModDefines)
                    {
                        foreach(var v in mod.ClassScalingResourceEntries)
                        {
                            foreach (var classFeature in v.ClassResourceFeatureGuids)
                            {
                                ModifyTools.RegisterForProcessing(v.Key, classFeature, true);
                            }
                            foreach (var nonClassFeature in v.NonClassResourceFeatureGuids)
                            {
                                ModifyTools.RegisterForProcessing(v.Key, nonClassFeature, false);
                            }
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
