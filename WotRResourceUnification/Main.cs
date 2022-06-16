using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TabletopTweaks.Core.Utilities;
using UnityModManagerNet;
using ResourceUnification.ModLogic;

namespace ResourceUnification
{
    static class Main
    {
        public static UnificationModContext Context;
        public static bool Enabled;
        static bool Load(UnityModManager.ModEntry modEntry)
        {
            try
            {
                Enabled = true;

                var harmony = new Harmony(modEntry.Info.Id);
                Context = new (modEntry);
                Context.ModEntry.OnSaveGUI = OnSaveGUI;
                Context.ModEntry.OnGUI = UMMSettingsUI.OnGUI;

#if DEBUG
                Context.Debug = true;
                Context.Blueprints.Debug = true;
#endif

                harmony.PatchAll();

                PostPatchInitializer.Initialize(Context);

                return true;
            }
            catch (Exception e)
            {
                Main.Context.Logger.LogError(e, e.Message);
                return false;
            }
        }

        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            Context.SaveAllSettings();



        }
    }
}
