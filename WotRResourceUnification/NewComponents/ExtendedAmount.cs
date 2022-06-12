using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WotRResourceUnification.NewComponents
{
    [AllowedOn(typeof(BlueprintAbilityResource))]
    [AllowMultipleComponents]
    public class ExtendedAmount : BlueprintComponent
    {
        

        public BlueprintAbilityResource.Amount Amount;

        public BlueprintArchetypeReference[] ArchetypesBlocked;

        public BlueprintArchetypeReference[] ArchetypesBlockedDiv;
    }
}
