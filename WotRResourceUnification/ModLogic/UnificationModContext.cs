using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TabletopTweaks.Core.ModLogic;
using UnityModManagerNet;
using ResourceUnification.Config;

namespace ResourceUnification.ModLogic
{
    class UnificationModContext : ModContextBase
    {
        public ResourceDefines ResourceDefines;

        public UnificationModContext(UnityModManager.ModEntry modEntry) : base(modEntry)
        {
            LoadBlueprints("ResourceUnification.Config", this);
            LoadSettings("ResourceDefines.json", "ResourceUnification.Config", ref ResourceDefines);
            
       }

       
        

        public override void LoadAllSettings()
        {
            base.AfterBlueprintCachePatches();
        }
        public override void AfterBlueprintCachePatches()
        {
            base.AfterBlueprintCachePatches();
            if (Debug)
            {
                Blueprints.RemoveUnused();
                SaveSettings(BlueprintsFile, Blueprints);
                
            }
        }

        public override void SaveAllSettings()
        {
            base.SaveAllSettings();
           
        }
    }
}
