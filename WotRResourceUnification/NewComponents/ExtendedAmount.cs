using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
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
    public class ImprovedAbilityResourceCalc : BlueprintComponent
    {
        public int BaseValue = 0;

        public int MinClassLevelIncrease;

        public bool UsesStat;

        

        public float OtherClassesModifier = 0f;

        public Dictionary<BlueprintCharacterClassReference, ClassEntry> classEntries = new();

        

        

       
        
    }

    

    public class ClassEntry
    {
        public VanillaEntry vanilla;
        public List<ArchetypeEntry> archetypeEntries = new();
        public BlueprintCharacterClassReference classRef;
        
        public ClassEntry(BlueprintCharacterClassReference characterClassReference)
        {
            classRef = characterClassReference;
        }
    }
        


    public class ArchetypeEntry : ClassGainSubEntry
    {
        public List<BlueprintArchetypeReference> Archetypes = new();

        public override bool Applies(ClassData d)
        {
            return d.Archetypes.Any(x => Archetypes.Contains(x.ToReference<BlueprintArchetypeReference>()));
        }
    }

    public class VanillaEntry : ClassGainSubEntry
    {
        public List<BlueprintArchetypeReference> BlockedArchetypes = new();

        public override bool Applies(ClassData d)
        {
            return !d.Archetypes.Any(x => BlockedArchetypes.Contains(x.ToReference<BlueprintArchetypeReference>()));
        }
    }

    public abstract class ClassGainSubEntry
    {
        public abstract bool Applies(ClassData d);

        public bool PerLevel;

        public int IncreasePerTick = 1;

        public int StartLevel;

        public int StartIncrease;


        private int levelStep = 1;

        public BlueprintFeatureReference m_UnlockFeature;

        public int LevelStep { get => levelStep != 0 ? levelStep : 1; set => levelStep = value; }
    }
}
