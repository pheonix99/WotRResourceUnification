using Kingmaker.Blueprints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResourceUnification.Content
{
    public class ResourceFeatureInfo
    {
        public BlueprintFeatureReference Feature;
        public string Key;
        public bool FromClass;
        public BlueprintAbilityResourceReference Resource;
        public BlueprintAbilityResource.Amount Amount;


        public ResourceFeatureInfo(string key, BlueprintFeatureReference feature,  bool fromClass)
        {
            Feature = feature;
            Key = key;
            FromClass = fromClass;
        }
    }
}
