using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Stats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ResourceUnification.NewComponents;

namespace ResourceUnification.Content
{
    public class LevelScalingUnification
    {
        public string Name;
        public BlueprintFeatureReference BaseFeature;
        public BlueprintAbilityResourceReference BaseResource;
        public StatType BaseFeatureStat;
        public bool UsesStat;
        public List<BlueprintAbilityResourceReference> UnifiedResources = new();
        public Dictionary<BlueprintFeatureReference, StatType> AltStatUnlocks = new();

        public List<BlueprintFeatureReference> ResourceFeatures = new();

        public LevelScalingUnification(string name, BlueprintFeatureReference baseFeature, BlueprintAbilityResourceReference baseResource, bool usesStat, StatType baseFeatureStat)
        {
            Name = name;
            BaseFeature = baseFeature;
            ResourceFeatures.Add(baseFeature);
            BaseResource = baseResource;
            UsesStat = usesStat;
            BaseFeatureStat = baseFeatureStat;
        }
    }
}
