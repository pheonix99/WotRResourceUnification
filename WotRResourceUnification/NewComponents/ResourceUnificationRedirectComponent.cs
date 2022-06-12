using Kingmaker.Blueprints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WotRResourceUnification.NewComponents
{
    [AllowedOn(typeof(BlueprintAbilityResource))]
    public class ResourceUnificationRedirectComponent : BlueprintComponent
    {
        
        public BlueprintAbilityResource RedirectTo
        {
            get
            {
                return m_RedirectTo?.Get();
            }
                
        }

        public BlueprintAbilityResourceReference m_RedirectTo;

    }
}
