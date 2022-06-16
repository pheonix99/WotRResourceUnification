using Kingmaker.Blueprints;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResourceUnification.NewComponents
{
    [AllowedOn(typeof(BlueprintFeature))]
    public class AddExistingAbilityResource : UnitFactComponentDelegate, ISubscriber, IUnitSubscriber, IUnitReapplyFeaturesOnLevelUpHandler
    {
        public BlueprintAbilityResourceReference m_Resource;
        public BlueprintAbilityResource.Amount m_MaxAmount;
        public void HandleUnitReapplyFeaturesOnLevelUp()
        {
            
        }
    }
}
