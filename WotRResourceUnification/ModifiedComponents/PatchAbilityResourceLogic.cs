using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.ActivatableAbilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ResourceUnification.NewComponents;

namespace ResourceUnification.ModifiedComponents
{
	
    class PatchAbilityResourceLogic
    {
		[HarmonyPatch(typeof(AbilityResourceLogic), "GetAbilityRestrictionUIText")]
		static class AbilityResourceLogic_ImproveLogic
		{
			public static void Postfix(ref string __result, AbilityResourceLogic __instance)
			{
				__result = __result + " : " + __instance.RequiredResource.LocalizedName;
			}
		}
		
		[HarmonyPatch(typeof(AbilityResourceLogic), "RequiredResource", MethodType.Getter)]
		static class AbilityResourceLogic_RedirectToUnifiedResource
		{
			public static void Postfix(ref BlueprintAbilityResource __result, AbilityResourceLogic __instance)
			{
				try
				{
					
					var redirect = __instance.m_RequiredResource.Get()?.Components.OfType<ResourceRedirectComponent>().FirstOrDefault();
					if (redirect != null && redirect.m_RedirectTo != null)
					{
						//Main.Context.Logger.Log($"Attempting Resource Redirect on {__instance.OwnerBlueprint.NameSafe()}");
						__result = redirect.RedirectTo;
						
					}
				}
				catch (Exception e)
				{
					Main.Context.Logger.LogError(e, "Error In AbilityResourceLogic_RedirectToUnifiedResource");
				}


				
			}
		}
			
		
		[HarmonyPatch(typeof(ActivatableAbilityResourceLogic), "RequiredResource", MethodType.Getter)]
		static class ActivatableAbilityResourceLogic_RedirectToUnifiedResource
		{
			public static void Postfix(ref BlueprintAbilityResource __result, ActivatableAbilityResourceLogic __instance)
			{
				try
				{
					
					var redirect = __instance.m_RequiredResource.Get()?.Components.OfType<ResourceRedirectComponent>().FirstOrDefault();
					if (redirect != null && redirect.m_RedirectTo != null)
					{
						//Main.Context.Logger.Log($"Attempting Resource Redirect on {__instance.OwnerBlueprint.NameSafe()}");
						__result = redirect.RedirectTo;
						
					}
				}
				catch (Exception e)
				{
					Main.Context.Logger.LogError(e, "Error In ActivatableAbilityResourceLogic_RedirectToUnifiedResource");
				}


				
				
			}
		}
		
	}
	
	
}
