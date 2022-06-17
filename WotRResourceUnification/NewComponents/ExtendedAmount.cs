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

        public int BaseValue = 0;

        public int StartIncrease;

        public int MinClassLevelIncrease;

        private int levelStep = 1;

        public BlueprintFeatureReference m_UnlockFeature;

        public int LevelStep { get => levelStep != 0 ? levelStep : 1; set => levelStep = value; }
        

        public string DisplayInfo()
        {
            StringBuilder report = new();
            if (BaseValue != 0)
                report.Append($"Base Value: {BaseValue}, ");
            
            if (PerLevel)
            {
                report.Append($"{IncreasePerTick} per level");
            }
            else
            {
                
                if (MinClassLevelIncrease != 0)
                {
                    report.Append($"Minimum Increase From Level: {MinClassLevelIncrease}. ");
                    
                }
                report.Append($"{(double)IncreasePerTick / (double)LevelStep} per level");
                if (StartLevel != 0)
                {
                    report.Append($" counting from {StartLevel}");
                }
             
                if (StartIncrease != 0)
                {
                    report.Append($"with {StartIncrease} starting bonus");
                }
                report.Append(".");


            }


            return report.ToString();
        }
    }
}
