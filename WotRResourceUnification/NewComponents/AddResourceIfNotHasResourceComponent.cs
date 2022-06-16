using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.EntitySystem;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResourceUnification.NewComponents
{
    [AllowedOn(typeof(BlueprintFeature))]
    public class AddFeatureIfHasResourceFromClass : UnitFactComponentDelegate<AddFeatureIfHasResourceFromClassData>, IOwnerGainLevelHandler, IUnitSubscriber, ISubscriber
    {

        public BlueprintAbilityReference m_AbilityResource;
        public BlueprintFeatureReference m_Feature;
        public bool Not;
        public override void OnActivate()
        {
            this.Apply();
        }

        // Token: 0x0600C314 RID: 49940 RVA: 0x00314CD3 File Offset: 0x00312ED3
        public override void OnDeactivate()
        {
            base.Owner.RemoveFact(base.Data.AppliedFact);
            base.Data.AppliedFact = null;
        }

        // Token: 0x0600C315 RID: 49941 RVA: 0x00314CCB File Offset: 0x00312ECB
        public void HandleUnitGainLevel()
        {
            this.Apply();
        }

        // Token: 0x0600C316 RID: 49942 RVA: 0x00314CF8 File Offset: 0x00312EF8
        private void Apply()
        {
            if (base.Data.AppliedFact != null)
            {
                return;
            }
            var resource = base.Owner.Resources.GetResource(m_AbilityResource);
            if (resource == null && Not)
            {
                base.Data.AppliedFact = base.Owner.AddFact(this.m_Feature.Get(), null, null);
            }
            else if (resource != null)
            {
                var sourceINfo = resource.Blueprint.Components.OfType<ResourceSourceInfoComponent>().Where(x => x.IsClassFeature && Owner.Progression.Features.HasFact(x.Unlock));
                if (Not && !sourceINfo.Any())
                {
                    base.Data.AppliedFact = base.Owner.AddFact(this.m_Feature.Get(), null, null);
                }
                else if (!Not && sourceINfo.Any())
                {
                    base.Data.AppliedFact = base.Owner.AddFact(this.m_Feature.Get(), null, null);
                }
                
               
               
            }
        }
    }

    public class AddFeatureIfHasResourceFromClassData
    {
        // Token: 0x04007F5A RID: 32602
        [JsonProperty]
        public EntityFact AppliedFact;
    }
}
