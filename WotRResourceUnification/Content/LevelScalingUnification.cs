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
        public ResourceFeatureInfo BaseInfo;
        
        public bool UsesStat;
        public List<BlueprintAbilityResourceReference> ResourcesCovered = new();
        public Dictionary<BlueprintFeatureReference, StatType> StatUnlocks = new();

        public List<ResourceFeatureInfo> ProcessedResourceFeatures = new();
        public List<ResourceFeatureInfo> UnprocessedResourceAddingFeatures = new();
        

     


        public LevelScalingUnification(string name)
        {
            Name = name;
        }
    }
}
