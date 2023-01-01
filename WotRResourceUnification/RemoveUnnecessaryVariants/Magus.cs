using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using ResourceUnification.NewComponents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TabletopTweaks.Core.Utilities;

namespace ResourceUnification.RemoveUnnecessaryVariants
{
    class Magus
    {
        public static void RemoveEldritchArcanaSelector()
        {
            if (Main.Context.Config.RemoveUnnecessaryVariants.IsDisabled("EldritchArcana"))
                return;

            BlueprintArchetype escion = BlueprintTools.GetBlueprint<BlueprintArchetype>("d078b2ef073f2814c9e338a789d97b73");
            var eArcan = BlueprintTools.GetBlueprintReference<BlueprintFeatureBaseReference>("d4b54d9db4932454ab2899f931c2042c");
            var nArcan = BlueprintTools.GetBlueprintReference<BlueprintFeatureBaseReference>("e9dc4dfc73eaaf94aae27e0ed6cc9ada");
            int[] arcanaLevels = new int[] { 3, 6, 9, 12, 15, 18 };
            foreach (int i in arcanaLevels)
            {
                escion.AddFeatures.FirstOrDefault(x => x.Level == i).m_Features.Remove(eArcan);
                escion.RemoveFeatures.FirstOrDefault(x => x.Level == i).m_Features.Remove(nArcan);
            }
            var nArcans = BlueprintTools.GetBlueprint<BlueprintFeatureSelection>("e9dc4dfc73eaaf94aae27e0ed6cc9ada");
            nArcans.m_AllFeatures = nArcans.m_AllFeatures.AppendToArray(BlueprintTools.GetBlueprintReference<BlueprintFeatureReference>("d88e219124a9e5d4da4908f599aef7aa"));

            var EldritchWandMaster = BlueprintTools.GetBlueprint<BlueprintFeature>("d88e219124a9e5d4da4908f599aef7aa");
            EldritchWandMaster.AddComponent<PrequisiteUsesStatForSpellbook>(x =>
            {
                x.m_class = escion.m_ParentClass.ToReference<BlueprintCharacterClassReference>();
                x.RequiredStat = Kingmaker.EntitySystem.Stats.StatType.Charisma;

            });
            var WandMaster = BlueprintTools.GetBlueprint<BlueprintFeature>("1b94043744e83494c8a083319e1602f3");
            WandMaster.AddComponent<PrequisiteUsesStatForSpellbook>(x =>
            {
                x.m_class = escion.m_ParentClass.ToReference<BlueprintCharacterClassReference>();
                x.RequiredStat = Kingmaker.EntitySystem.Stats.StatType.Intelligence;

            });


        }
    }
}
