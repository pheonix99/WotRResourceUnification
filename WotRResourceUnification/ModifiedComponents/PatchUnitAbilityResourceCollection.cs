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
                    if (blueprint is null)
                    {
                        Main.Context.Logger.LogError($"Null BP passed to RedirectGetResource");
                        return;
                    }

                    ResourceRedirectComponent redirect = blueprint.Components?.OfType<ResourceRedirectComponent>().FirstOrDefault();
                    if (redirect?.RedirectTo != null)
                    {
#if DEBUG
                        //Main.Context.Logger.Log($"UnitAbilityResourceCollection_RedirectGetResource executing for {blueprint.name} on {__instance.m_Owner.CharacterName}");
#endif
                        if (__result != null && redirect.RedirectTo != null && __result.Blueprint.AssetGuid.Equals(redirect.RedirectTo.AssetGuid))
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
#if DEBUG
                                Main.Context.Logger.LogError($"UnitAbilityResourceCollection_RedirectGetResource redirected to null resource for {blueprint.NameSafe()} on {__instance.m_Owner.CharacterName}, redirectto is  {redirect.RedirectTo?.name ?? "null"}, original result is {__result?.Blueprint?.name ?? "null"}, {blueprint.name} is {(__instance.m_Resources.ContainsKey(blueprint) ? "present" : "not present")}");

#endif
                                

                                        Main.Context.Logger.Log($"Patching {redirect.RedirectTo} onto {__instance.m_Owner.CharacterName}");
                                        var newResource = (__instance.m_Resources[redirect.RedirectTo] = new UnitAbilityResource(redirect.RedirectTo));
                                        newResource.Retain();
                                        newResource.Amount = newResource.GetMaxAmount(__instance.m_Owner);
                                        __result = newResource;



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
#if DEBUG
                        Main.Context.Logger.Log($"UnitAbilityResourceCollection_RedirectGetResource redirecting {blueprint.name} to {redirect.RedirectTo.name} instance is {__result.Blueprint.name}");
#endif
                    }
                }
                catch(Exception e)
                {
                    try
                    {
                        Main.Context.Logger.LogError(e, $"Error In RedirectGetResource for {blueprint?.name ?? "Blueprint Missing!"}");
                    }
                    catch (Exception e2)
                    {
                        Main.Context.Logger.LogError(e, $"Error In RedirectGetResource killed logger!");
                        Main.Context.Logger.LogError(e2, $"Logger kill data!");
                    }
                    
                }

                

            }
        }
#if DEBUG
        [HarmonyPatch(typeof(UnitAbilityResourceCollection), "Add")]
        static class SeeAddCall
        {



            public static bool Prefix(UnitAbilityResourceCollection __instance, BlueprintScriptableObject blueprint, bool restoreAmount)
            {
                Main.Context.Logger.Log($"Prefix: Add called for {blueprint.NameSafe()}, redirected resource is {__instance.GetResource(blueprint)?.Blueprint?.name ?? "null name"}");
                /*
                if (blueprint is BlueprintAbilityResource resource)
                {
                    var redirect = resource.GetComponent<ResourceRedirectComponent>();
                    if (redirect?.RedirectTo != null)
                    {
                        if (!redirect.RedirectTo.Equals(resource))
                        {
                            Main.Context.Logger.Log($"Redirecting Add from {blueprint.name} to {redirect.RedirectTo.name}");
                            blueprint = redirect.RedirectTo;
                        }
                        else
                        {
                            Main.Context.Logger.Log($"Not redirecting add from {blueprint.name} to {redirect.RedirectTo.name}");
                        }
                    }
                }
              */
                


                return true;

            }

    
        }
#endif

        //Everything else runs throuigh that
    }
}
