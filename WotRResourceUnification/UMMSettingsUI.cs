﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityModManagerNet;
using ResourceUnification.Content;
using TabletopTweaks.Core.UMMTools;
using ResourceUnification.NewComponents;


namespace ResourceUnification
{
    class UMMSettingsUI
    {
        private static int selectedTab;
        public static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            UI.AutoWidth();
            UI.TabBar(ref selectedTab,
                () => UI.Label(""),
                    new NamedAction("Unification Info", () => InfoTabls.Info()),
                    new NamedAction("Configuration", () => SettingsTabs.Config())
                    );

        }

        static class InfoTabls
        {
            private static int selectedSubTab;
            public static void Info()
            {
                SetttingUI.TabLevel TabLevel = SetttingUI.TabLevel.Zero;
               
                //UI.Div(0, 15);
                UI.TabBar(ref selectedSubTab, () =>  UI.Label("Note: Entries show state of mod system, non-mod resource handling is not shown.") , ModifyTools.Unifications.Select(x => new NamedAction(x.Name, () =>
                {

                    ImprovedAbilityResourceCalc extended = x.BaseInfo.Resource.Get().Components.OfType<ImprovedAbilityResourceCalc>().FirstOrDefault();
                    if (extended != null)
                    {
                        foreach (KeyValuePair<Kingmaker.Blueprints.BlueprintCharacterClassReference, ClassEntry> element in extended.classEntries)
                        {
                            UI.Label($"Class Entry: {element.Key.NameSafe()}".green().bold());
                            UI.Div(10, 15);
                            if (element.Value.vanilla != null)
                            {
                                UI.Label($"Base Class Resource Gain: {element.Value.vanilla.DisplayInfo()} Not for archetypes: {ModifyTools.MakeArcheLog(element.Value.vanilla.BlockedArchetypes)}");
                            }
                            else
                            {
                                UI.Label($"No Gain For Vanilla Version");
                            }
                            foreach(ArchetypeEntry entry in element.Value.archetypeEntries)
                            {
                                UI.Label($"{ModifyTools.MakeArcheLog(entry.Archetypes)} Resource Gain: {entry.DisplayInfo()} ");
                            }
                        }

                    }
                    else
                    {
                        UI.Label("Failed to load data");
                    }
                    
                })).ToArray());

            }
        }

        static class SettingsTabs
        {

            public static void Config()
            {
                SetttingUI.TabLevel TabLevel = SetttingUI.TabLevel.Zero;
                Config.Config config = Main.Context.Config;
                //UI.Div(0, 15);
                using (UI.VerticalScope())
                {
                    UI.Toggle("New Settings Off By Default".bold(), ref config.NewSettingsOffByDefault);
                    UI.Space(25);
                    SetttingUI.SettingGroup("Remove Unnecessary Variants", TabLevel, config.RemoveUnnecessaryVariants);
                    

                }
            }

        }

    }


}
