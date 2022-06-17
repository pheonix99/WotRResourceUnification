using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TabletopTweaks.Core.Config;

namespace ResourceUnification.Config
{
    class ResourceDefines : IUpdatableSettings 
    {
        
        public List<CombinedScalingResourceEntry> ClassScalingResourceEntries = new();

        public void Init()
        {
         
        }

        public void OverrideSettings(IUpdatableSettings userSettings)
        {
            var loadedSettings = userSettings as ResourceDefines;
            if (loadedSettings == null) { return; }
        }
    }
    public class CombinedScalingResourceEntry
    {
        public string Key;
        
        public List<GameResourceEntry> ClassResourceFeatureGuids;
        public List<GameResourceEntry> NonClassResourceFeatureGuids;
    }
    public class GameResourceEntry
    {
        public string ResourceAdderFeatureGuid;
        public string WrapperGuid;
       
    }
}
