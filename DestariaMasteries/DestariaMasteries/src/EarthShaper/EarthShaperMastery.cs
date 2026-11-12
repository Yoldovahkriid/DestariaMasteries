using MasteryLibrary.src.Core.Masteries.Data;
using MasteryLibrary.src.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace DestariaMasteries.src.EarthShaper
{
    public class EarthShaperMastery : Mastery
    {
        public override string Code => "Earthshaper";
        public override int MaxLevel => 30;
        public EarthShaperMastery()
        {
            var earthMoverSkill = new EarthMoverSkill();
            Skills.Add(earthMoverSkill.Code, earthMoverSkill);

            var panHandlingSkill = new PanHandlingSkill();
            Skills.Add(panHandlingSkill.Code, panHandlingSkill);

            var timeToRockSkill = new TimeToRockSkill();
            Skills.Add(timeToRockSkill.Code, timeToRockSkill);
        }
    }

    public class EarthMoverSkill: Skill
    {
        public override string Code => "EarthMover";
        public override int MaxLevel => 5;
        public override int Column => 2;
        public override EnumSkillType SkillType => EnumSkillType.Passive;
        public override int RequiredMasteryLevel => 1;
        public EarthMoverSkill()
        {
            Attributes["PassiveStats"] = new Dictionary<string, StatConfiguration>
            {
                ["soilBreakingSpeedMul"] = new("Linear", new() { ["BaseValue"] = 0.1f }),
            };
        }
    }

    public class PanHandlingSkill : Skill
    {
        public override string Code => "PanHandling";
        public override int MaxLevel => 5;
        public override int Column => 2;
        public override EnumSkillType SkillType => EnumSkillType.Passive;
        public override int RequiredMasteryLevel => 1;
        public PanHandlingSkill()
        {
            Attributes["PassiveStats"] = new Dictionary<string, StatConfiguration>
            {
                ["panSaturationConsumptionMul"] = new("Array", new() { ["Values"] = new float[] { 1f } }),
                ["panDropMul"] = new("Array", new() { ["Values"] = new float[] { 0.25f, 0.3f, 0.35f, 0.4f, 0.5f } }),
            };
        }
    }

    public class TimeToRockSkill : Skill
    {
        public override string Code => "TimeToRock";
        public override int MaxLevel => 3;
        public override int Column => 3;
        public override EnumSkillType SkillType => EnumSkillType.Active;
        public override float Cooldown => 60;
        public override string Ability => "TimeToRock";
        public override int RequiredMasteryLevel => 1;
        public TimeToRockSkill()
        {
            Attributes["PassiveStats"] = new Dictionary<string, StatConfiguration>
            {
                ["stoneToolDurabilityMul"] = new("Array", new() { ["Values"] = new float[] { 0.5f, 1.0f, 2.0f } }),
            };
        }
    }
}
