using HarmonyLib;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UnitLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WotRResourceUnification.NewComponents;

namespace WotRResourceUnification.ModifiedComponents
{

	class PatchBlueprintAbilityResource
	{
		[HarmonyPatch(typeof(BlueprintAbilityResource), "GetMaxAmount")]
		static class ApplyAltStatsToMax
		{

			[HarmonyPriority(Priority.Normal)]
			public static void Postfix(ref int __result, BlueprintAbilityResource __instance, UnitDescriptor unit)
			{


				try
				{


					if (__instance.Components.OfType<AltAbilityBonusStatUnlockComponent>().Any(x => x.Active(unit)))
					{
						Main.Context.Logger.Log($"ApplyAltStatsToMax executing for {__instance.name} on {unit.CharacterName}, entering with {__result}");
						int reduce = 0;
						if (__instance.m_MaxAmount.IncreasedByStat)//Compute base stat result
						{
							ModifiableValueAttributeStat modifiableValueAttributeStat = unit.Stats.GetStat(__instance.m_MaxAmount.ResourceBonusStat) as ModifiableValueAttributeStat;
							if (modifiableValueAttributeStat != null)
							{
								reduce += modifiableValueAttributeStat.Bonus;
								Main.Context.Logger.Log($"ApplyAltStatsToMax executing for {__instance.name} on {unit.CharacterName}, reducing by {reduce} to cancel {__instance.m_MaxAmount.ResourceBonusStat}");
							}
							else
							{

							}
						}
						__result -= reduce;
						int increase = 0;
						foreach (var t in __instance.Components.OfType<AltAbilityBonusStatUnlockComponent>().Where(x => x.Active(unit)))
						{
							int statVal = (unit.Stats.GetStat(t.AltStat) as ModifiableValueAttributeStat).Bonus;
							increase = Math.Max((statVal), increase);
							Main.Context.Logger.Log($"ApplyAltStatsToMax executing for {__instance.name} on {unit.CharacterName}, alt stat candidate {t.AltStat} from {t.name} offering {statVal}");
						}
						__result += increase;
						Main.Context.Logger.Log($"ApplyAltStatsToMax executing for {__instance.name} on {unit.CharacterName}, final selected increase is {increase}, total is {__result}");


					}
				}
				catch (Exception e)
				{
					Main.Context.Logger.LogError(e, "Error In ApplyAltStatsToMax");
				}


			}


		}

		[HarmonyPatch(typeof(BlueprintAbilityResource), "GetMaxAmount")]
		static class BlueprintAbilityResource_RedirectToUnifiedResource
		{
			public static bool Prefix(ref int __result, BlueprintAbilityResource __instance, UnitDescriptor unit)
			{
				try
				{
					var redirect = __instance.Components.OfType<ResourceUnificationRedirectComponent>().FirstOrDefault();
					if (redirect != null)
					{
						Main.Context.Logger.Log($"BlueprintAbilityResource_RedirectToUnifiedResource executing for {__instance.Name} on {unit.CharacterName}");
						__result = redirect.RedirectTo.GetMaxAmount(unit);
						return false;
					}
				}
				catch (Exception e)
				{
					Main.Context.Logger.LogError(e, "Error In BlueprintAbilityResource_RedirectToUnifiedResource");
				}


				return true;
			}
		}

	}
	
}
