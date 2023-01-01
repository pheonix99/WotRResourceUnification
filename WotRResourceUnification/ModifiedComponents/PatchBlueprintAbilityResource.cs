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
using TabletopTweaks.Core.Utilities;

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
#if DEBUG
						Main.Context.Logger.Log($"ImprovedAbilityResourceCalc for {__instance.name} on {unit.CharacterName} gives {__result} in postfix");
#endif
					}
					else if (__instance.GetComponents<ResourceSourceInfoComponent>().Any(x => x.Active(unit)))
					{
#if DEBUG
						Main.Context.Logger.Log($"ApplyAltStatsToMax executing for {__instance.name} on {unit.CharacterName}, entering with {__result}");
#endif
						int reduce = 0;
						if (__instance.m_MaxAmount.IncreasedByStat)//Compute base stat result
						{
							ModifiableValueAttributeStat modifiableValueAttributeStat = unit.Stats.GetStat(__instance.m_MaxAmount.ResourceBonusStat) as ModifiableValueAttributeStat;
							if (modifiableValueAttributeStat != null)
							{
								reduce += modifiableValueAttributeStat.Bonus;
#if DEBUG
								Main.Context.Logger.Log($"ApplyAltStatsToMax executing for {__instance.name} on {unit.CharacterName}, reducing by {reduce} to cancel {__instance.m_MaxAmount.ResourceBonusStat}");
#endif
							}
							else
							{

							}
						}
						__result -= reduce;
						int increase = 0;
						foreach (ResourceSourceInfoComponent t in __instance.GetComponents<ResourceSourceInfoComponent>(x => x.Active(unit)))
						{
							int statVal = (unit.Stats.GetStat(t.AltStat) as ModifiableValueAttributeStat).Bonus;
							increase = Math.Max((statVal), increase);
#if DEBUG
							Main.Context.Logger.Log($"ApplyAltStatsToMax executing for {__instance.name} on {unit.CharacterName}, alt stat candidate {t.AltStat} from {t.name} offering {statVal}");
#endif
						}
						__result += increase;
#if DEBUG
						Main.Context.Logger.Log($"ApplyAltStatsToMax executing for {__instance.name} on {unit.CharacterName}, final selected increase is {increase}, total is {__result}");
#endif


					}
				}
				catch (Exception e)
				{
					Main.Context.Logger.LogError(e, $"Error In ApplyAltStatsToMax on {__instance?.name ?? "Blueprint Missing!"}");
				}


			}


		}

		[HarmonyPatch(typeof(BlueprintAbilityResource), "GetMaxAmount")]
		static class BlueprintAbilityResource_ExecuteCustomComputation
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



                    ImprovedAbilityResourceCalc customHandler = __instance.GetComponent<ImprovedAbilityResourceCalc>();
					if (customHandler != null)
                    {
#if DEBUG
						Main.Context.Logger.Log($"Starting Prefix Custom Logic on {__instance.name} with stat logic");
#endif
						double runningTotal = 0;
						
						float otherClassMultiplier = customHandler.OtherClassesModifier;
						if (customHandler.UsesStat || __instance.Components.OfType<ResourceSourceInfoComponent>().Any())
                        {
							int increase = 0;
							foreach (ResourceSourceInfoComponent t in __instance.GetComponents<ResourceSourceInfoComponent>(x => x.Active(unit)))
							{

								int stat = (unit.Stats.GetStat(t.AltStat) as ModifiableValueAttributeStat).ModifiedValue;
								int statVal = (unit.Stats.GetStat(t.AltStat) as ModifiableValueAttributeStat).Bonus;
							
								increase = Math.Max((statVal), increase);
#if DEBUG
								Main.Context.Logger.Log($"Prefix ver ApplyAltStatsToMax executing for {__instance.name} on {unit.CharacterName}, alt stat candidate {t.AltStat} {stat} from {t.m_Unlock.NameSafe()} offering {statVal}");
#endif
							}
							runningTotal += increase;
						}
#if DEBUG
						Main.Context.Logger.Log($"Moving to level logic on {__instance.name} with stat logic");
#endif
						
						int bestBaseValue = 0;
						int bestMinClassValue = 0;
						double classRunningTotal = 0;
						foreach (ClassData charClass in unit.Progression.Classes)
						{
#if DEBUG
							Main.Context.Logger.Log($"Assessing {charClass.CharacterClass.Name} on {unit.CharacterName}");
							
#endif
							int found = 0;
							double best = 0;
							if (customHandler.classEntries.TryGetValue(charClass.CharacterClass.ToReference<BlueprintCharacterClassReference>(), out ClassEntry classEntry))
                            {

#if DEBUG
								Main.Context.Logger.Log($"ClassEntry found");
#endif
                                List<ClassGainSubEntry> entries = new List<ClassGainSubEntry>();
								foreach(ArchetypeEntry v in classEntry.archetypeEntries)
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
#if DEBUG
									Main.Context.Logger.Log($"Applicable entries for {charClass.CharacterClass.Name} not found on  on {__instance.name}");
#endif
								}
								else
								{


									foreach (ClassGainSubEntry entry in entries)
									{
										bestBaseValue = Math.Max(bestBaseValue, entry.BaseValue);
										if (entry.PerLevel)
										{
											best = Math.Max(best, entry.IncreasePerTick * charClass.Level);
										}
										else
                                        {
#if DEBUG
											Main.Context.Logger.Log($"LevelStep is {entry.LevelStep}");
#endif

											bestMinClassValue = Math.Max(entry.MinClassLevelIncrease, bestMinClassValue);
											best = Math.Max(best, (double)entry.IncreasePerTick * ((double)charClass.Level - (double)entry.StartLevel) / (double)entry.LevelStep);
											if (charClass.Level >= entry.StartLevel)
                                            {
												best += entry.StartIncrease;
                                            }
                                        }
									}
								}
                            }
                            else
                            {
#if DEBUG
								Main.Context.Logger.Log($"Entry for {charClass.CharacterClass.Name} not found on  on {__instance.name}");
#endif
								best += charClass.Level * otherClassMultiplier;
							}
							classRunningTotal += best;
							
#if DEBUG
							Main.Context.Logger.Log($"Custom class calc logic executing for {__instance.name} on {unit.CharacterName}, class {charClass.CharacterClass.Name} provided {best}: total is {runningTotal}");
#endif
						}

						runningTotal += (double)bestBaseValue;
						runningTotal += Math.Max(classRunningTotal, (double) bestMinClassValue);
						int bonus = 0;
						EventBus.RaiseEvent<IResourceAmountBonusHandler>(unit.Unit, delegate (IResourceAmountBonusHandler h)
						{
							h.CalculateMaxResourceAmount(__instance, ref bonus);
						});
						__result = Math.Max(__instance.m_Min, __instance.ApplyMinMax((int)runningTotal) + bonus);
#if DEBUG
						Main.Context.Logger.Log($"Custom class calc logic executing for {__instance.name} on {unit.CharacterName}, total is {__result}");
#endif
						return false;
					}
					else
                    {
						return true;
                    }
				}
				catch (Exception e)
				{
					Main.Context.Logger.LogError(e, $"Error In BlueprintAbilityResource_RedirectToUnifiedResource on {__instance?.name ?? "Blueprint Missing!"}");
				}


				return true;
			}
		}

	}
	
}
