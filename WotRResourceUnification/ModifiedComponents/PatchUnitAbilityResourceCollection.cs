using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.UnitLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WotRResourceUnification.NewComponents;

namespace WotRResourceUnification.ModifiedComponents
{
    public class PatchUnitAbilityResourceCollection
    {



        [HarmonyPatch(typeof(UnitAbilityResourceCollection), "GetResource")]
        static class RedirectGetResource
        {



            public static bool Prefix(UnitAbilityResource __result, UnitAbilityResourceCollection __instance, BlueprintScriptableObject blueprint)
            {
                try
                {
                    var redirect = blueprint.Components.OfType<ResourceUnificationRedirectComponent>().FirstOrDefault();
                    if (redirect != null)
                    {
                        //Main.Context.Logger.Log($"BlueprintAbilityResource_RedirectToUnifiedResource executing for {blueprint.NameSafe()} on {__instance.m_Owner.CharacterName}");

                        __instance.m_Resources.TryGetValue(redirect.RedirectTo, out var tempResult);
                        __result = tempResult;
                        return false;
                    }
                }
                catch(Exception e)
                {
                    Main.Context.Logger.LogError(e, "Error In RedirectGetResource");
                }

                return true;

            }
        }

        //Everything else runs throuigh that
    }
}
