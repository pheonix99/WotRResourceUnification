using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TabletopTweaks.Core.Config;

namespace WotRResourceUnification.Config
{
    class ResourceDefines : IUpdatableSettings 
    {
        
        public List<CombinedResourceEntry> ResourceEntries = new();

        public void Init()
        {
         
        }

        public void OverrideSettings(IUpdatableSettings userSettings)
        {
            var loadedSettings = userSettings as ResourceDefines;
            if (loadedSettings == null) { return; }
        }
    }
    public class CombinedResourceEntry
    {
        public string Key;
        public string BaseResourceFeatureGuid;
        public List<string> AltResourceFeatureGuids;
    }
    public class GameResourceEntry
    {

    }
}
