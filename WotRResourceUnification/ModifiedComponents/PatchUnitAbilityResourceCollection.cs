using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.UnitLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ResourceUnification.NewComponents;

namespace ResourceUnification.ModifiedComponents
{
    public class PatchUnitAbilityResourceCollection
    {



        [HarmonyPatch(typeof(UnitAbilityResourceCollection), "GetResource")]
        static class RedirectGetResource
        {



            public static void Postfix(UnitAbilityResource __result, UnitAbilityResourceCollection __instance, BlueprintScriptableObject blueprint)
            {
                try
                {
                    var redirect = blueprint.Components.OfType<ResourceRedirectComponent>().FirstOrDefault();
                    if (redirect != null)
                    {
#if DEBUG
                        //Main.Context.Logger.Log($"UnitAbilityResourceCollection_RedirectGetResource executing for {blueprint.NameSafe()} on {__instance.m_Owner.CharacterName}");
#endif


                        __instance.m_Resources.TryGetValue(redirect.RedirectTo as BlueprintScriptableObject, out var tempResult);;
                        if (tempResult == null)
                        {
                            Main.Context.Logger.LogError($"UnitAbilityResourceCollection_RedirectGetResource redirected to null resource for {blueprint.NameSafe()} on {__instance.m_Owner.CharacterName}");
                            
                           
                        }
                        else
                        {
                            __result = tempResult;
                            
                        }
                    }
                }
                catch(Exception e)
                {
                    Main.Context.Logger.LogError(e, "Error In RedirectGetResource");
                }

                

            }
        }

        [HarmonyPatch(typeof(UnitAbilityResourceCollection), "Add")]
        static class SeeAddCall
        {



            public static bool Prefix(UnitAbilityResourceCollection __instance, BlueprintScriptableObject blueprint, bool restoreAmount)
            {

#if DEBUG
                Main.Context.Logger.Log($"Prefix: Add called for {blueprint.NameSafe()}, redirected resource is {__instance.GetResource(blueprint)?.Blueprint?.NameSafe()}");
#endif

                return true;

            }

    
        }


        //Everything else runs throuigh that
    }
}
