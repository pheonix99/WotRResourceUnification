using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.Designers.Mechanics.Facts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ResourceUnification.NewComponents;

namespace ResourceUnification.ModifiedComponents
{
    class PatchAddAbilityResources
	{       
		[HarmonyPatch(typeof(AddAbilityResources), "Resource", MethodType.Getter)]
		static class AddAbilityResources_RedirectToUnifiedResource
		{
			public static void Postfix(ref BlueprintAbilityResource __result, AddAbilityResources __instance)
			{
				try
				{
					if (__instance.m_Resource is null)
					{
						Main.Context.Logger.LogError($"AddAbilityResources_RedirectToUnifiedResource: __instance.m_resource is null on {__instance?.OwnerBlueprint?.name ?? "NO BLUEPRINT"}");
						return;
                    }



                    ResourceRedirectComponent redirect = __instance.m_Resource?.Get()?.GetComponent<ResourceRedirectComponent>();
					if (redirect != null && redirect.m_RedirectTo != null)
					{
#if DEBUG
						Main.Context.Logger.Log($"Attempting AddAbilityResources Redirect on {__instance.OwnerBlueprint.NameSafe()} from {__instance.m_Resource.NameSafe()} redirect target is {redirect.RedirectTo.NameSafe()}");
#endif
						__result = redirect.RedirectTo;
						
					}
				}
				catch (Exception e)
				{
					Main.Context.Logger.LogError(e, $"Error In AddAbilityResources_RedirectToUnifiedResource  on {__instance?.OwnerBlueprint?.name ?? "NO BLUEPRINT"}");
				}


				
			}
		}
			

	}
}
