using MasteryLibrary.src.Core.Masteries.Data;
using MasteryLibrary.src.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Config;

namespace DestariaMasteries.src.Mechanist
{
    public class MechanistMastery : Mastery
    {
        public override string Code => "Mechanist";
        public override int MaxLevel => 30;

        public MechanistMastery()
        {
            var combatEngineering = new CombatEngineeringSkill();
            this.Skills.Add(combatEngineering.Code, combatEngineering);

            var tinkering = new TinkeringSkill();
            this.Skills.Add(tinkering.Code, tinkering);

            var delicateTouch = new DelicateTouchSkill();
            this.Skills.Add(delicateTouch.Code, delicateTouch);

            var tuning = new TuningSkill();
            this.Skills.Add(tuning.Code, tuning);

            var beastTamer = new BeastTamerSkill();
            this.Skills.Add(beastTamer.Code, beastTamer);

            var beastSlayer = new BeastSlayerSkill();
            this.Skills.Add(beastSlayer.Code, beastSlayer);

            var efficientConstruction = new EfficientConstructionSkill();
            this.Skills.Add(efficientConstruction.Code, efficientConstruction);

            var eyeForCraftsmanship = new EyeForCraftsmanshipSkill();
            this.Skills.Add(eyeForCraftsmanship.Code, eyeForCraftsmanship);

            var scrapMechanic = new ScrapMechanicSkill();
            this.Skills.Add(scrapMechanic.Code, scrapMechanic);

            var antiArmor = new AntiArmorSkill();
            this.Skills.Add(antiArmor.Code, antiArmor);

            var temporalAdjustment = new TemporalAdjustmentSkill();
            this.Skills.Add(temporalAdjustment.Code, temporalAdjustment);

            var temporalInurement = new TemporalInurementSkill();
            this.Skills.Add(temporalInurement.Code, temporalInurement);

            var antiAnything = new AntiAnythingSkill();
            this.Skills.Add(antiAnything.Code, antiAnything);

            var productTesting = new ProductTestingSkill();
            this.Skills.Add(productTesting.Code, productTesting);

            var refinedDevelopment = new RefinedDevelopmentSkill();
            this.Skills.Add(refinedDevelopment.Code, refinedDevelopment);

            var wardagainstbeasts = new WardAgainstBeastsSkill();
            this.Skills.Add(wardagainstbeasts.Code, wardagainstbeasts);

            var turnbacktheclock = new TurnBackTheClockSkill();
            this.Skills.Add(turnbacktheclock.Code, turnbacktheclock);

            var stormchaser = new StormChaserSkill();
            this.Skills.Add(stormchaser.Code, stormchaser);

            var temporaldevastation = new TemporalDevastationSkill();
            this.Skills.Add(temporaldevastation.Code, temporaldevastation);
        }

        public override string GetDisplayName(int level)
        {
            return Lang.Get("destariamasteries:mechanist-mastery-name");
        }

        public override string GetDescription(int level)
        {
            return Lang.Get("destariamasteries:mechanist-mastery-desc");
        }
    }

    public class  CombatEngineeringSkill : Skill
    {
        public override string Code => "CombatEngineering";
        public override int MaxLevel => 5;
        public override int Column => 6;
        public override EnumSkillType SkillType => EnumSkillType.Passive;

        public CombatEngineeringSkill()
        {
            var parameters = new Dictionary<string, object> { { "BaseValue", 0.02f } };
            var passivestats = new Dictionary<string, StatConfiguration> { { "crossbowDamageMultiplier", new StatConfiguration("Linear", parameters) } };
            Attributes.Add("PassiveStats", passivestats);
        }

        public override string GetDisplayName(int level)
        {
            return Lang.Get("destariamasteries:mechanist-combatengineering-name");
        }

        public override string GetDescription(int level)
        {
            StringBuilder craftableCrossbows = new StringBuilder();
            craftableCrossbows.Append("Simple Crossbows, ");
            if (level >= 3)
            {
                craftableCrossbows.Append("Repeating Crossbows,");
            }
            if (level == 5)
            {
                craftableCrossbows.Append("Latch Crossbows");
            }
            var passiveStats = GetValueSafe<Dictionary<string, StatConfiguration>>("PassiveStats");
            float crossbowDmgMult = StatScalingUtil.GetScaledValue(passiveStats?.GetValueOrDefault("crossbowDamageMultiplier"), level);
            return Lang.Get("destariamasteries:mechanist-combatengineering-desc", craftableCrossbows.ToString().TrimEnd(',', ' '), crossbowDmgMult * 100);
        }
    }

    public class TinkeringSkill : Skill
    {
        public override string Code => "Tinkering";
        public override int MaxLevel => 5;
        public override int Column => 0;
        public override EnumSkillType SkillType => EnumSkillType.Passive;

        public TinkeringSkill()
        {
            var parameters = new Dictionary<string, object> { { "BaseValue", 0.02f } };
            var passivestats = new Dictionary<string, StatConfiguration> { { "mechanicalsDamage", new StatConfiguration("Linear", parameters) } };
            Attributes.Add("PassiveStats", passivestats);
        }
        public override string GetDisplayName(int level)
        {
            return Lang.Get("destariamasteries:mechanist-tinkering-name");
        }

        public override string GetDescription(int level)
        {
            string mechanicalitems = "Gain the ability to craft chutes, hoppers, axles, and angled gears.";
            if (level >= 3)
            {
                mechanicalitems = "Gain the ability to craft helve hammers, pounders, basic windmills, and their remaining associated parts (for example, big gears, sails";
            }
            if (level == 5)
            {
                mechanicalitems = "Gain the ability to craft reinforced rotors and various other assorted mechanical parts and machines. (for example, clutches, brakes, and any mod machines)";
            }
            var passiveStats = GetValueSafe<Dictionary<string, StatConfiguration>>("PassiveStats");
            float mechanicalsDmgMult = StatScalingUtil.GetScaledValue(passiveStats?.GetValueOrDefault("mechanicalsDamage"), level);

            return Lang.Get("destariamasteries:mechanist-tinkering-desc", mechanicalitems, mechanicalsDmgMult * 100);
        }
    }

    public class DelicateTouchSkill : Skill
    {
        public override string Code => "DelicateTouch";
        public override int MaxLevel => 5;
        public override int RequiredMasteryLevel => 6;
        public override int Column => 2;
        public override EnumSkillType SkillType => EnumSkillType.Passive;
        public DelicateTouchSkill()
        {
            var parameters = new Dictionary<string, object> { { "BaseValue", 0.12f } };
            var passivestats = new Dictionary<string, StatConfiguration> { { "smallToolDamageReduction", new StatConfiguration("Linear", parameters) } };
            Attributes.Add("PassiveStats", passivestats);
        }
        public override string GetDisplayName(int level)
        {
            return Lang.Get("destariamasteries:mechanist-delicatetouch-name");
        }
        public override string GetDescription(int level)
        {
            var passiveStats = GetValueSafe<Dictionary<string, StatConfiguration>>("PassiveStats");
            float smallToolDmgReduction = StatScalingUtil.GetScaledValue(passiveStats?.GetValueOrDefault("smallToolDamageReduction"), level);
            return Lang.Get("destariamasteries:mechanist-delicatetouch-desc", smallToolDmgReduction * 100);
        }
    }

    public class TuningSkill : Skill
    {
        public override string Code => "Tuning";
        public override int MaxLevel => 1;
        public override int RequiredMasteryLevel => 6;
        public override int Column => 4;
        public override EnumSkillType SkillType => EnumSkillType.Passive;
        public TuningSkill()
        {
        }
        public override string GetDisplayName(int level)
        {
            return Lang.Get("destariamasteries:mechanist-tuning-name");
        }
        public override string GetDescription(int level)
        {
            return Lang.Get("destariamasteries:mechanist-tuning-desc");
        }
    }

    public class BeastTamerSkill : Skill
    {
        public override string Code => "BeastTamer";
        public override int MaxLevel => 3;
        public override int RequiredMasteryLevel => 6;
        public override int Column => 3;
        public override EnumSkillType SkillType => EnumSkillType.Passive;

        public override string? ExclusiveGroup => "BeastSkills";
        public BeastTamerSkill()
        {
            var parameters = new Dictionary<string, object> { { "BaseValue", -0.33f } };
            var passivestats = new Dictionary<string, StatConfiguration> { { "TuningSpearDamageAgainstMechanicals", new StatConfiguration("Linear", parameters) } };
            Attributes.Add("PassiveStats", passivestats);

            LevelRequirements = new Dictionary<int, List<SkillPrerequisite>>
            {
                {1, new List<SkillPrerequisite> { new SkillPrerequisite { SkillCode = "Tuning", MinimumLevel = 1 } } }
            };
        }
        public override string GetDisplayName(int level)
        {
            return Lang.Get("destariamasteries:mechanist-beasttamer-name");
        }
        public override string GetDescription(int level)
        {
            string bonusText = "";
            if (level == 3)
            {
                bonusText = "Gain the ability to hack a corrupt sawblade locust";
            }
            var passiveStats = GetValueSafe<Dictionary<string, StatConfiguration>>("PassiveStats");
            float tuningSpearDmgAgainstMechanicals = StatScalingUtil.GetScaledValue(passiveStats?.GetValueOrDefault("TuningSpearDamageAgainstMechanicals"), level);
            return Lang.Get("destariamasteries:mechanist-beasttamer-desc", tuningSpearDmgAgainstMechanicals * -100, bonusText);
        }
    }

    public class BeastSlayerSkill : Skill
    {
        public override string Code => "BeastSlayer";
        public override int MaxLevel => 3;
        public override int RequiredMasteryLevel => 6;
        public override int Column => 5;
        public override EnumSkillType SkillType => EnumSkillType.Passive;
        public override string? ExclusiveGroup => "BeastSkills";
        public BeastSlayerSkill()
        {
            var parameters = new Dictionary<string, object> { { "Values", new float[] { 1.25f, 2.5f, 4.0f } } };
            var passivestats = new Dictionary<string, StatConfiguration> { { "TuningSpearDamageAgainstMechanicals", new StatConfiguration("Array", parameters) } };
            Attributes.Add("PassiveStats", passivestats);

            LevelRequirements = new Dictionary<int, List<SkillPrerequisite>>
            {
                {1, new List<SkillPrerequisite> { new SkillPrerequisite { SkillCode = "Tuning", MinimumLevel = 1 } } }
            };
        }
        public override string GetDisplayName(int level)
        {
            return Lang.Get("destariamasteries:mechanist-beastslayer-name");
        }
        public override string GetDescription(int level)
        {
            var passiveStats = GetValueSafe<Dictionary<string, StatConfiguration>>("PassiveStats");
            float tuningSpearDmgAgainstMechanicals = StatScalingUtil.GetScaledValue(passiveStats?.GetValueOrDefault("TuningSpearDamageAgainstMechanicals"), level);
            return Lang.Get("destariamasteries:mechanist-beastslayer-desc", tuningSpearDmgAgainstMechanicals * 100);
        }
    }

    public class EfficientConstructionSkill : Skill
    {
        public override string Code => "EfficientConstruction";
        public override int MaxLevel => 5;
        public override int RequiredMasteryLevel => 11;
        public override int Column => 1;
        public override EnumSkillType SkillType => EnumSkillType.Passive;
        public EfficientConstructionSkill()
        {
            LevelRequirements = new Dictionary<int, List<SkillPrerequisite>>
            {
                {1, new List<SkillPrerequisite> { new SkillPrerequisite { SkillCode = "Tinkering", MinimumLevel = 5 } } }
            };
        }
        public override string GetDisplayName(int level)
        {
            return Lang.Get("destariamasteries:mechanist-efficientconstruction-name");
        }
        public override string GetDescription(int level)
        {
            return Lang.Get("destariamasteries:mechanist-efficientconstruction-desc");
        }
    }

    public class EyeForCraftsmanshipSkill : Skill
    {
        public override string Code => "EyeForCraftsmanship";
        public override int MaxLevel => 5;
        public override int RequiredMasteryLevel => 11;
        public override int Column => 3;
        public override EnumSkillType SkillType => EnumSkillType.Passive;
        public EyeForCraftsmanshipSkill()
        {
            var parameters = new Dictionary<string, object> { { "BaseValue", 0.05f } };
            var glassesbonus = new Dictionary<string, StatConfiguration> { { "glassesBonus", new StatConfiguration("Linear", parameters) } };
            Attributes.Add("glassesbonus", glassesbonus);
        }
        public override string GetDisplayName(int level)
        {
            return Lang.Get("destariamasteries:mechanist-eyeforcraftsmanship-name");
        }
        public override string GetDescription(int level)
        {

            var glassesBonus = GetValueSafe<Dictionary<string, StatConfiguration>>("glassesbonus");
            float bonus = StatScalingUtil.GetScaledValue(glassesBonus?.GetValueOrDefault("glassesBonus"), level);
            return Lang.Get("destariamasteries:mechanist-eyeforcraftsmanship-desc", bonus * 100);
        }
    }

    public class ScrapMechanicSkill : Skill
    {
        public override string Code => "ScrapMechanic";
        public override int MaxLevel => 3;
        public override int RequiredMasteryLevel => 11;
        public override int Column => 2;
        public override EnumSkillType SkillType => EnumSkillType.Active;
        public override string Ability => "ScrapMechanic";
        public override float Cooldown => 3600;
        public ScrapMechanicSkill()
        {
        }
        public override string GetDisplayName(int level)
        {
            return Lang.Get("destariamasteries:mechanist-scrapmechanics-name");
        }
        public override string GetDescription(int level)
        {
            return Lang.Get("destariamasteries:mechanist-scrapmechanics-desc", level, level);
        }
    }

    public class AntiArmorSkill : Skill
    {
        public override string Code => "AntiArmor";
        public override int MaxLevel => 1;
        public override int RequiredMasteryLevel => 16;
        public override int Column => 6;
        public override EnumSkillType SkillType => EnumSkillType.Passive;
        public AntiArmorSkill()
        {
            LevelRequirements = new Dictionary<int, List<SkillPrerequisite>>
            {
                {1, new List<SkillPrerequisite> { new SkillPrerequisite { SkillCode = "CombatEngineering", MinimumLevel = 5 } } }
            };
        }
        public override string GetDisplayName(int level)
        {
            return Lang.Get("destariamasteries:mechanist-antiarmor-name");
        }
        public override string GetDescription(int level)
        {
            return Lang.Get("destariamasteries:mechanist-antiarmor-desc");
        }
    }

    public class TemporalAdjustmentSkill : Skill
    {
        public override string Code => "TemporalAdjustment";
        public override int MaxLevel => 1;
        public override int RequiredMasteryLevel => 16;
        public override int Column => 2;
        public override EnumSkillType SkillType => EnumSkillType.Active;
        public override float Cooldown => 72000;
        public override string Ability => "TemporalAdjustment";

        public TemporalAdjustmentSkill()
        {
            var parameters = new Dictionary<string, object> { { "BaseValue", -1.0f } };
            var passivestats = new Dictionary<string, StatConfiguration> { { "temporalGearTLRepairCost", new StatConfiguration("Linear", parameters) } };
            Attributes.Add("PassiveStats", passivestats);
        }
        public override string GetDisplayName(int level)
        {
            return Lang.Get("destariamasteries:mechanist-temporaladjustment-name");
        }
        public override string GetDescription(int level)
        {
            return Lang.Get("destariamasteries:mechanist-temporaladjustment-desc");
        }
    }

    public class TemporalInurementSkill : Skill
    {
        public override string Code => "TemporalInurement";
        public override int MaxLevel => 5;
        public override int RequiredMasteryLevel => 16;
        public override int Column => 3;
        public override EnumSkillType SkillType => EnumSkillType.Passive;
        public TemporalInurementSkill()
        {
            var parameters = new Dictionary<string, object> { { "BaseValue", -0.1f } };
            var passivestats = new Dictionary<string, StatConfiguration> { { "temporalStabilityDropRate", new StatConfiguration("Linear", parameters) } };
            Attributes.Add("PassiveStats", passivestats);
        }
        public override string GetDisplayName(int level)
        {
            return Lang.Get("destariamasteries:mechanist-temporalinurement-name");
        }
        public override string GetDescription(int level)
        {
            var passiveStats = GetValueSafe<Dictionary<string, StatConfiguration>>("PassiveStats");
            float temporalStabilityDropRate = StatScalingUtil.GetScaledValue(passiveStats?.GetValueOrDefault("temporalStabilityDropRate"), level);
            return Lang.Get("destariamasteries:mechanist-temporalinurement-desc", temporalStabilityDropRate * 100);
        }
    }

    public class AntiAnythingSkill : Skill
    {
        public override string Code => "AntiAnything";
        public override int MaxLevel => 1;
        public override int RequiredMasteryLevel => 21;
        public override int Column => 6;
        public override EnumSkillType SkillType => EnumSkillType.Passive;
        public AntiAnythingSkill()
        {
            LevelRequirements = new Dictionary<int, List<SkillPrerequisite>>
            {
                {1, new List<SkillPrerequisite> { new SkillPrerequisite { SkillCode = "AntiArmor", MinimumLevel = 1 } } }
            };
        }
        public override string GetDisplayName(int level)
        {
            return Lang.Get("destariamasteries:mechanist-antianything-name");
        }
        public override string GetDescription(int level)
        {
            return Lang.Get("destariamasteries:mechanist-antianything-desc");
        }
    }

    public class ProductTestingSkill : Skill
    {
        public override string Code => "ProductTesting";
        public override int MaxLevel => 5;
        public override int RequiredMasteryLevel => 21;
        public override int Column => 5;
        public override EnumSkillType SkillType => EnumSkillType.Passive;
        public ProductTestingSkill()
        {
            var parameters = new Dictionary<string, object> { { "BaseValue", -0.06f } };
            var passivestats = new Dictionary<string, StatConfiguration> { { "crossbowDispersion", new StatConfiguration("Linear", parameters) } };
            var parameters2 = new Dictionary<string, object> { { "BaseValue", 0.02f } };
            passivestats.Add("crossbowDamageMultiplier", new StatConfiguration("Linear", parameters2));
            Attributes.Add("PassiveStats", passivestats);

            LevelRequirements = new Dictionary<int, List<SkillPrerequisite>>
            {
                {1, new List<SkillPrerequisite> { new SkillPrerequisite { SkillCode = "CombatEngineering", MinimumLevel = 1 } } }
            };
        }
        public override string GetDisplayName(int level)
        {
            return Lang.Get("destariamasteries:mechanist-producttesting-name");
        }
        public override string GetDescription(int level)
        {
            var passiveStats = GetValueSafe<Dictionary<string, StatConfiguration>>("PassiveStats");
            float crossbowDispersion = StatScalingUtil.GetScaledValue(passiveStats?.GetValueOrDefault("crossbowDispersion"), level);
            float crossbowDamageMultiplier = StatScalingUtil.GetScaledValue(passiveStats?.GetValueOrDefault("crossbowDamageMultiplier"), level);
            return Lang.Get("destariamasteries:mechanist-producttesting-desc", crossbowDispersion * 100, crossbowDamageMultiplier * 100);
        }
    }

    public class RefinedDevelopmentSkill : Skill
    {
        public override string Code => "RefinedDevelopment";
        public override int MaxLevel => 3;
        public override int RequiredMasteryLevel => 21;
        public override int Column => 0;
        public override EnumSkillType SkillType => EnumSkillType.Passive;
        public RefinedDevelopmentSkill()
        {
            LevelRequirements = new Dictionary<int, List<SkillPrerequisite>>
            {
                {1, new List<SkillPrerequisite> { new SkillPrerequisite { SkillCode = "Tinkering", MinimumLevel = 5 } } }
            };
        }
        public override string GetDisplayName(int level)
        {
            return Lang.Get("destariamasteries:mechanist-refineddevelopment-name");
        }
        public override string GetDescription(int level)
        {
            return Lang.Get("destariamasteries:mechanist-refineddevelopment-desc");
        }
    }

    public class WardAgainstBeastsSkill : Skill
    {
        public override string Code => "WardAgainstBeast";
        public override int MaxLevel => 5;
        public override int RequiredMasteryLevel => 26;
        public override int Column => 0;
        public override EnumSkillType SkillType => EnumSkillType.Passive;
        public WardAgainstBeastsSkill()
        {
            var parameters = new Dictionary<string, object> { { "BaseValue", -0.1f } };
            var passivestats = new Dictionary<string, StatConfiguration> { { "damageResistanceAgainstMechanicals", new StatConfiguration("Linear", parameters) } };
            var parameters2 = new Dictionary<string, object> { { "BaseValue", -0.02f } };
            passivestats.Add("damageResistanceAgainstRust", new StatConfiguration("Linear", parameters2));
            Attributes.Add("PassiveStats", passivestats);
        }
        public override string GetDisplayName(int level)
        {
            return Lang.Get("destariamasteries:mechanist-wardagainstbeasts-name");
        }
        public override string GetDescription(int level)
        {
            var passiveStats = GetValueSafe<Dictionary<string, StatConfiguration>>("PassiveStats");
            float damageResistanceAgainstMechanicals = StatScalingUtil.GetScaledValue(passiveStats?.GetValueOrDefault("damageResistanceAgainstMechanicals"), level);
            float damageResistanceAgainstRust = StatScalingUtil.GetScaledValue(passiveStats?.GetValueOrDefault("damageResistanceAgainstRust"), level);

            return Lang.Get("destariamasteries:mechanist-wardagainstbeasts-desc", damageResistanceAgainstMechanicals * -100, damageResistanceAgainstRust * -100);
        }
    }

    public class TurnBackTheClockSkill : Skill
    {
        public override string Code => "TurnBackTheClock";
        public override int MaxLevel => 5;
        public override int RequiredMasteryLevel => 30;
        public override int Column => 0;
        public override EnumSkillType SkillType => EnumSkillType.Passive;
        public TurnBackTheClockSkill()
        {
            var parameters = new Dictionary<string, object> { { "Values", new float[] { 0.05f, 0.1f, 0.25f, 0.5f, 1.0f } } };
            var passivestats = new Dictionary<string, StatConfiguration> { { "deathHealthRefund", new StatConfiguration("Array", parameters) } };
            Attributes.Add("PassiveStats", passivestats);
        }

        public override string GetDisplayName(int level)
        {
            return Lang.Get("destariamasteries:mechanist-turnbacktheclock-name");
        }
        public override string GetDescription(int level)
        {
            var passiveStats = GetValueSafe<Dictionary<string, StatConfiguration>>("PassiveStats");
            float healthRefund = StatScalingUtil.GetScaledValue(passiveStats?.GetValueOrDefault("deathHealthRefund"), level);
            return Lang.Get("destariamasteries:mechanist-turnbacktheclock-desc", healthRefund * 100);
        }
    }

    public class StormChaserSkill : Skill
    {
        public override string Code => "StormChaser";
        public override int MaxLevel => 1;
        public override int RequiredMasteryLevel => 30;
        public override int Column => 2;
        public override EnumSkillType SkillType => EnumSkillType.Active;
        public override string Ability => "StormChaser";
        public StormChaserSkill()
        {

        }

        public override string GetDisplayName(int level)
        {
            return Lang.Get("destariamasteries:mechanist-stormchaser-name");
        }
        public override string GetDescription(int level)
        {
            return Lang.Get("destariamasteries:mechanist-stormchaser-desc");
        }
    }

    public class TemporalDevastationSkill : Skill
    {
        public override string Code => "TemporalDevastation";
        public override int MaxLevel => 5;
        public override int RequiredMasteryLevel => 30;
        public override int Column => 6;
        public override EnumSkillType SkillType => EnumSkillType.Active;
        public override string Ability => "TemporalDevastation";
        public override float Cooldown => 1800;
        public TemporalDevastationSkill()
        {
            var skilldamage = new Dictionary<string, object> { { "Values", new float[] { 15, 30, 45, 60, 80 } } };
            var lostStability = new Dictionary<string, object> { { "Values", new float[] { 0.2f, 0.4f, 0.6f, 0.8f, 1.2f } } };
            var slowedamount = new Dictionary<string, object> { { "Values", new float[] { -0.1f, -0.2f, -0.3f, -0.4f, -0.6f } } };
            Attributes.Add("SkillDamage", new StatConfiguration("Array", skilldamage));
            Attributes.Add("LostStability", new StatConfiguration("Array", lostStability));
            Attributes.Add("SlowAmount", new StatConfiguration("Array", slowedamount));
            Attributes.Add("SkillRange", 12);
        }

        public override string GetDisplayName(int level)
        {
            return Lang.Get("destariamasteries:mechanist-temporaldevastation-name");
        }
        public override string GetDescription(int level)
        {
            StatConfiguration damage = GetValueSafe<StatConfiguration>("SkillDamage");
            StatConfiguration loststability = GetValueSafe<StatConfiguration>("LostStability");
            StatConfiguration slowAmount = GetValueSafe<StatConfiguration>("SlowAmount");
            return Lang.Get("destariamasteries:mechanist-temporaldevastation-desc", StatScalingUtil.GetScaledValue(damage, level) * 100, StatScalingUtil.GetScaledValue(loststability, level) * 100, StatScalingUtil.GetScaledValue(slowAmount, level) * 100);
        }
    }
}
