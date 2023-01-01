using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TabletopTweaks.Core.Config;

namespace ResourceUnification.Config
{
    class Config : IUpdatableSettings
    {
        public bool NewSettingsOffByDefault = false;
        public SettingGroup RemoveUnnecessaryVariants;

        public void Init()
        {
            
        }

        public void OverrideSettings(IUpdatableSettings userSettings)
        {
            Config loadedSettings = userSettings as Config;
            NewSettingsOffByDefault = loadedSettings.NewSettingsOffByDefault;
            RemoveUnnecessaryVariants.LoadSettingGroup(loadedSettings.RemoveUnnecessaryVariants, NewSettingsOffByDefault);
        }
    }

}
