using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResourceUnification.NewComponents
{
    class PrequisiteUsesStatForSpellbook : Prerequisite
    {
        public override bool CheckInternal([CanBeNull] FeatureSelectionState selectionState, [NotNull] UnitDescriptor unit, [CanBeNull] LevelUpState state)
        {
            return RequiredStat.Equals(UnitsAttribute(unit));
        }

        private StatType UnitsAttribute(UnitDescriptor unit)
        {
            return (unit.GetSpellbook(Class)?.Blueprint.CastingAttribute ?? StatType.Unknown);
        }

        public override string GetUITextInternal(UnitDescriptor unit)
        {
            return $"Casts {Class.Name} spells using {RequiredStat}";
        }

        public StatType RequiredStat;

        public BlueprintCharacterClass Class
        {
            get => m_class.Get();
        }

        public BlueprintCharacterClassReference m_class;
    }
}
