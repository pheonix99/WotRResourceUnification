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
                    ResourceRedirectComponent redirect = blueprint.Components.OfType<ResourceRedirectComponent>().FirstOrDefault();
                    if (redirect != null)
                    {
#if DEBUG
                        //Main.Context.Logger.Log($"UnitAbilityResourceCollection_RedirectGetResource executing for {blueprint.NameSafe()} on {__instance.m_Owner.CharacterName}");
#endif
                        if (__result != null && redirect.RedirectTo != null && __result.Blueprint.Equals(redirect.RedirectTo))
                        {
#if DEBUG
                            Main.Context.Logger.Log($"No need to redirect on {blueprint.NameSafe()}");


#endif
                            return;
                        }

                        __instance.m_Resources.TryGetValue(redirect.RedirectTo as BlueprintScriptableObject, out UnitAbilityResource tempResult);;
                        if (tempResult == null)
                        {
                            if (Kingmaker.Game.Instance.LevelUpController == null)
                            {
                                Main.Context.Logger.LogError($"UnitAbilityResourceCollection_RedirectGetResource redirected to null resource for {blueprint.NameSafe()} on {__instance.m_Owner.CharacterName}");
                            }
                            else
                            {
#if DEBUG
                                Main.Context.Logger.LogError($"UnitAbilityResourceCollection_RedirectGetResource redirected to null resource for {blueprint.NameSafe()} on {__instance.m_Owner.CharacterName} - would be bypassed in live!");
#endif
                            }

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
#if DEBUG
        [HarmonyPatch(typeof(UnitAbilityResourceCollection), "Add")]
        static class SeeAddCall
        {



            public static bool Prefix(UnitAbilityResourceCollection __instance, BlueprintScriptableObject blueprint, bool restoreAmount)
            {


              
                Main.Context.Logger.Log($"Prefix: Add called for {blueprint.NameSafe()}, redirected resource is {__instance.GetResource(blueprint)?.Blueprint?.NameSafe()}");


                return true;

            }

    
        }
#endif

        //Everything else runs throuigh that
    }
}
