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
using TabletopTweaks.Core.Config;
using static UnityModManagerNet.UnityModManager;

namespace ResourceUnification.ModLogic
{
    class UnificationModContext : ModContextBase
    {
        public ResourceDefines ResourceDefines;
        public List<ResourceDefines> OtherModDefines = new();

        public UnificationModContext(UnityModManager.ModEntry modEntry) : base(modEntry)
        {
            LoadBlueprints("ResourceUnification.Config", this);
            LoadSettings("DefaultResourceDefines.json", "ResourceUnification.Config", ref ResourceDefines);

            foreach(var mod in UnityModManager.modEntries)
            {
                if (mod.Active)
                {
                    string path = mod.Path + Path.DirectorySeparatorChar + "UserSettings" + Path.DirectorySeparatorChar + "ResourceDefines.json";
                    if (File.Exists(Path.Combine(mod.Path + "ResourceDefines.json")))
                    {
                        var newDefine = new ResourceDefines();
                        LoadForeignSettings<ResourceDefines>("ResourceDefines.json", mod, ref newDefine);
                        OtherModDefines.Add(newDefine);
                    }
                }
            }
            
       }
        private static JsonSerializerSettings cachedSettings;
        private static JsonSerializerSettings SerializerSettings
        {
            get
            {
                if (cachedSettings == null)
                {
                    cachedSettings = new JsonSerializerSettings
                    {
                        CheckAdditionalContent = false,
                        ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
                        DefaultValueHandling = DefaultValueHandling.Include,
                        FloatParseHandling = FloatParseHandling.Double,
                        Formatting = Formatting.Indented,
                        MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead,
                        MissingMemberHandling = MissingMemberHandling.Ignore,
                        NullValueHandling = NullValueHandling.Include,
                        ObjectCreationHandling = ObjectCreationHandling.Replace,
                        StringEscapeHandling = StringEscapeHandling.Default,
                    };
                }
                return cachedSettings;
            }
        }

        public void LoadForeignSettings<T>(string fileName, ModEntry otherMod, ref T setting) where T : IUpdatableSettings
        {
            JsonSerializer serializer = JsonSerializer.Create(SerializerSettings);
            var assembly = ModEntry.Assembly;
           
            var userPath = $"{otherMod.Path}{Path.DirectorySeparatorChar}{fileName}";

            
            if (File.Exists(userPath))
            {
                using (StreamReader streamReader = File.OpenText(userPath))
                using (JsonReader jsonReader = new JsonTextReader(streamReader))
                {
                    try
                    {
                        setting.Init();
                        T userSettings = serializer.Deserialize<T>(jsonReader);
                        setting.OverrideSettings(userSettings);
                    }
                    catch
                    {
                        
                        
                    }
                }
            }
            
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
