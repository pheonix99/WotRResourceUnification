using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UnitLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResourceUnification.NewComponents
{
    [AllowedOn(typeof(BlueprintAbilityResource))]
    [AllowMultipleComponents]
    public class ResourceSourceInfoComponent : BlueprintComponent
    {
        public StatType AltStat;

        public bool IsClassFeature;

        public BlueprintFeature Unlock
        {
            get
            {
                return m_Unlock.Get();
            }
        }

        public BlueprintFeatureReference m_Unlock;

        public bool Active (UnitDescriptor unit)
        {
            return unit.Progression.Features.HasFact(Unlock);
        }

    }
}
