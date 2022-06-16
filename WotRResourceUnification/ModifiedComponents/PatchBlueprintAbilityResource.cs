using HarmonyLib;
using Kingmaker;
using Kingmaker.Armies.TacticalCombat;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ResourceUnification.NewComponents;

namespace ResourceUnification.ModifiedComponents
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
					
					if (__instance.Components.OfType<ImprovedAbilityResourceCalc>().Any())
                    {
						
                    }
					else if (__instance.Components.OfType<ResourceSourceInfoComponent>().Any(x => x.Active(unit)))
					{
						//Main.Context.Logger.Log($"ApplyAltStatsToMax executing for {__instance.name} on {unit.CharacterName}, entering with {__result}");
						int reduce = 0;
						if (__instance.m_MaxAmount.IncreasedByStat)//Compute base stat result
						{
							ModifiableValueAttributeStat modifiableValueAttributeStat = unit.Stats.GetStat(__instance.m_MaxAmount.ResourceBonusStat) as ModifiableValueAttributeStat;
							if (modifiableValueAttributeStat != null)
							{
								reduce += modifiableValueAttributeStat.Bonus;
								//Main.Context.Logger.Log($"ApplyAltStatsToMax executing for {__instance.name} on {unit.CharacterName}, reducing by {reduce} to cancel {__instance.m_MaxAmount.ResourceBonusStat}");
							}
							else
							{

							}
						}
						__result -= reduce;
						int increase = 0;
						foreach (var t in __instance.Components.OfType<ResourceSourceInfoComponent>().Where(x => x.Active(unit)))
						{
							int statVal = (unit.Stats.GetStat(t.AltStat) as ModifiableValueAttributeStat).Bonus;
							increase = Math.Max((statVal), increase);
							//Main.Context.Logger.Log($"ApplyAltStatsToMax executing for {__instance.name} on {unit.CharacterName}, alt stat candidate {t.AltStat} from {t.name} offering {statVal}");
						}
						__result += increase;
						//Main.Context.Logger.Log($"ApplyAltStatsToMax executing for {__instance.name} on {unit.CharacterName}, final selected increase is {increase}, total is {__result}");


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
			[HarmonyPriority(Priority.Normal)]
			public static bool Prefix(ref int __result, BlueprintAbilityResource __instance, UnitDescriptor unit)
			{
				

				try
				{
						
					if (TacticalCombatHelper.IsActive && BlueprintRoot.Instance.TacticalCombat.LeaderManaResource == __instance)
					{
						return true;
					}

					
					
					var customHandler = __instance.Components.OfType<ImprovedAbilityResourceCalc>().FirstOrDefault();
					if (customHandler != null)
                    {
						//Main.Context.Logger.Log($"Starting Prefix Custom Logic");
						double runningTotal = 0;
						
						float otherClassMultiplier = customHandler.OtherClassesModifier;
						if (customHandler.UsesStat || __instance.Components.OfType<ResourceSourceInfoComponent>().Any())
                        {
							int increase = 0;
							foreach (var t in __instance.Components.OfType<ResourceSourceInfoComponent>().Where(x => x.Active(unit)))
							{
								int statVal = (unit.Stats.GetStat(t.AltStat) as ModifiableValueAttributeStat).Bonus;
								increase = Math.Max((statVal), increase);
								//Main.Context.Logger.Log($"Prefix ver ApplyAltStatsToMax executing for {__instance.name} on {unit.CharacterName}, alt stat candidate {t.AltStat} from {t.m_Unlock.NameSafe()} offering {statVal}");
							}
							runningTotal += increase;
						}

						int bestStartingIncrease = customHandler.BaseValue;
						foreach (var charClass in unit.Progression.Classes)
						{
							//Main.Context.Logger.Log($"Assessing {charClass.CharacterClass.Name} on {unit.CharacterName}");
							int found = 0;
							double best = 0;
							if (customHandler.classEntries.TryGetValue(charClass.CharacterClass.ToReference<BlueprintCharacterClassReference>(), out var classEntry))
                            {

								//Main.Context.Logger.Log($"ClassEntry found");
								var entries = new List<ClassGainSubEntry>();
								foreach(var v in classEntry.archetypeEntries)
                                {
									if (v.Applies(charClass))
                                    {
										entries.Add(v);
                                    }
                                }
								if (classEntry.vanilla != null && classEntry.vanilla.Applies(charClass))
                                {
									entries.Add(classEntry.vanilla);
                                }


								
								found = entries.Count;
								if (found == 0)
								{
									best = charClass.Level * otherClassMultiplier;
									//Main.Context.Logger.Log($"Applicable entries for {charClass.CharacterClass.Name} not found on  on {__instance.name}");
								}
								else
								{


									foreach (var entry in entries)
									{
										bestStartingIncrease = Math.Max(bestStartingIncrease, entry.StartIncrease);
										if (entry.PerLevel)
										{
											best = Math.Max(best, entry.IncreasePerTick * charClass.Level);
										}
										else
                                        {
											//Main.Context.Logger.Log($"LevelStep is {entry.LevelStep}");
											best = Math.Max(best, (double)entry.IncreasePerTick * ((double)charClass.Level - (double)entry.StartLevel) / (double)entry.LevelStep);
                                        }
									}
								}
                            }
                            else
                            {
								//Main.Context.Logger.Log($"Entry for {charClass.CharacterClass.Name} not found on  on {__instance.name}");
								best += charClass.Level * otherClassMultiplier;
							}
							
							runningTotal += best;
							//Main.Context.Logger.Log($"Custom class calc logic executing for {__instance.name} on {unit.CharacterName}, class {charClass.CharacterClass.Name} provided {best}: total is {runningTotal}");
						}
						runningTotal += bestStartingIncrease;
						int bonus = 0;
						EventBus.RaiseEvent<IResourceAmountBonusHandler>(unit.Unit, delegate (IResourceAmountBonusHandler h)
						{
							h.CalculateMaxResourceAmount(__instance, ref bonus);
						});
						__result = Math.Max(__instance.m_Min, __instance.ApplyMinMax((int)runningTotal) + bonus);
						//Main.Context.Logger.Log($"Custom class calc logic executing for {__instance.name} on {unit.CharacterName}, total is {__result}");
						return false;
					}
					else
                    {
						return true;
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
