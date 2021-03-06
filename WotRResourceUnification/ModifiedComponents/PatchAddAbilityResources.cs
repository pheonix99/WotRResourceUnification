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
		static class AbilityResourceLogic_RedirectToUnifiedResource
		{
			public static void Postfix(ref BlueprintAbilityResource __result, AddAbilityResources __instance)
			{
				try
				{
					
					var redirect = __instance.m_Resource.Get()?.Components.OfType<ResourceRedirectComponent>().FirstOrDefault();
					if (redirect != null && redirect.m_RedirectTo != null)
					{
#if DEBUG
						Main.Context.Logger.Log($"Attempting Resource Redirect on {__instance.OwnerBlueprint.NameSafe()}, redirect target is {redirect.RedirectTo.NameSafe()}");
#endif
						__result = redirect.RedirectTo;
						
					}
				}
				catch (Exception e)
				{
					Main.Context.Logger.LogError(e, "Error In AbilityResourceLogic_RedirectToUnifiedResource");
				}


				
			}
		}
			

	}
}
