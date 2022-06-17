using Kingmaker.Blueprints;
using ResourceUnification.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TabletopTweaks.Core.Utilities;

namespace ResourceUnification.Content
{
    public class ResourceFeatureInfo
    {
        public readonly BlueprintFeatureReference ResourceHoldingFeature;
        private readonly BlueprintFeatureReference _progressionFacingFeature;
        public BlueprintFeatureReference ProgressionFacingFeature => _progressionFacingFeature ?? ResourceHoldingFeature;
        public GameResourceEntry GameResourceEntry;

        public readonly string Key;
        public bool FromClass;
        public BlueprintAbilityResourceReference Resource;
        public BlueprintAbilityResource.Amount Amount;


        public ResourceFeatureInfo(string key, BlueprintFeatureReference resourceHoldingFeature, bool fromClass, BlueprintFeatureReference progressionFacingFeature = null)
        {
            ResourceHoldingFeature = resourceHoldingFeature;
            _progressionFacingFeature = progressionFacingFeature;
            Key = key;
            FromClass = fromClass;
        }
    }
}
