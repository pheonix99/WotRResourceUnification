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
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Class.Kineticist;

namespace ResourceUnification.ModifiedComponents
{
	
    class PatchAbilityResourceLogic
    {
		[HarmonyPatch(typeof(AbilityResourceLogic), "GetAbilityRestrictionUIText")]
		static class AbilityResourceLogic_ImproveLogic
		{
			public static void Postfix(ref string __result, AbilityResourceLogic __instance)
			{
				string name = __instance.RequiredResource.LocalizedName;
				if (String.IsNullOrWhiteSpace(name))
					name = __instance.RequiredResource.NameSafe();
#if DEBUG
				name = __instance.RequiredResource.NameSafe();
#endif


				__result = __result + " : " + name;
			}
		}
		
		[HarmonyPatch(typeof(AbilityResourceLogic), "RequiredResource", MethodType.Getter)]
		static class AbilityResourceLogic_RedirectToUnifiedResource
		{
			public static void Postfix(ref BlueprintAbilityResource __result, AbilityResourceLogic __instance)
			{
				try
				{

					if (__instance.m_RequiredResource == null)
                    {
						if (__instance.OwnerBlueprint?.GetComponent<AbilityKineticist>() is not null)
							return;//Don't bother logging if it's this weirdo kin stuff

						Main.Context.Logger.LogError($"__instance.m_RequiredResource is null in AbilityResourceLogic_RedirectToUnifiedResource!{__instance?.OwnerBlueprint?.name ?? "Blueprint Missing!"}");
						return;
                    }


					ResourceRedirectComponent redirect = __instance.m_RequiredResource?.Get()?.Components?.OfType<ResourceRedirectComponent>().FirstOrDefault();
					if (redirect != null && redirect.m_RedirectTo != null)
					{
						//Main.Context.Logger.Log($"Redirecting from {__result.NameSafe()} {__result.AssetGuidThreadSafe} to {redirect.RedirectTo.NameSafe()} {redirect.RedirectTo.AssetGuidThreadSafe}");

						__result = redirect.RedirectTo;
						
					}
				}
				catch (Exception e)
				{
					try
					{
						Main.Context.Logger.LogError(e, $"Error In AbilityResourceLogic_RedirectToUnifiedResource for {__instance?.OwnerBlueprint?.name ?? "Blueprint Missing!"}");
					}
					catch (Exception e2)
					{
						Main.Context.Logger.LogError(e, $"Error In AbilityResourceLogic_RedirectToUnifiedResource killed logger!");
						Main.Context.Logger.LogError(e2, $"Logger kill data!");
					}
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
					if (__instance.m_RequiredResource == null)
					{
						if (__instance.OwnerBlueprint?.GetComponent<AbilityKineticist>() is not null)
							return;//Don't bother logging if it's this weirdo kin stuff

						//Main.Context.Logger.LogError($"__instance.m_RequiredResource is null in ActivatableAbilityResourceLogic_RedirectToUnifiedResource on {__instance?.OwnerBlueprint?.name ?? "Blueprint Missing!"}");
						return;
					}

					ResourceRedirectComponent redirect = __instance.m_RequiredResource.Get()?.Components?.OfType<ResourceRedirectComponent>().FirstOrDefault();
					if (redirect != null && redirect.m_RedirectTo != null)
					{

						__result = redirect.RedirectTo;
						
					}
				}
				catch (Exception e)
				{
					try
					{
						Main.Context.Logger.LogError(e, $"Error In ActivatableAbilityResourceLogic_RedirectToUnifiedResource for {__instance?.OwnerBlueprint?.name ?? "Blueprint Missing!"}");
					}
					catch (Exception e2)
					{
						Main.Context.Logger.LogError(e, $"Error In ActivatableAbilityResourceLogic_RedirectToUnifiedResource killed logger!");
						Main.Context.Logger.LogError(e2, $"Logger kill data!");
					}
					
				}


				
				
			}
		}
		
	}
	
	
}
