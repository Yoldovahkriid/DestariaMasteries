using DestariaMasteries.src.Utils;
using MasteryLibrary.src.Core.Masteries.Data;
using MasteryLibrary.src.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Config;

namespace DestariaMasteries.src.Homesteader
{
    public class HomesteaderMastery : Mastery
    {
        public override string Code => "Homesteader";
        public override int MaxLevel => 30;
        public HomesteaderMastery() { 
            var subsistanceSkill = new SubsistanceSkill();
            this.Skills.Add(subsistanceSkill.Code, subsistanceSkill);

            var amberCombsSkill = new AmberCombsSkill();
            this.Skills.Add(amberCombsSkill.Code, amberCombsSkill);

            var emeraldBoughsSkill = new EmeraldBoughsSkill();
            this.Skills.Add(emeraldBoughsSkill.Code, emeraldBoughsSkill);

            var goldenFieldsSkill = new GoldenFieldsSkill();
            this.Skills.Add(goldenFieldsSkill.Code, goldenFieldsSkill);

            var deepBlueSkill = new DeepBlueSkill();
            this.Skills.Add(deepBlueSkill.Code, deepBlueSkill);

            var whispersInRedSkill = new WhispersInRed();
            this.Skills.Add(whispersInRedSkill.Code, whispersInRedSkill);

            var selfsufficientSkill = new SelfsufficientSkill();
            this.Skills.Add(selfsufficientSkill.Code, selfsufficientSkill);

            var bitFruitySkill = new BitFruitySkill();
            this.Skills.Add(bitFruitySkill.Code, bitFruitySkill);

            var heartyVeggiesSkill = new HeartyVeggiesSkill();
            this.Skills.Add(heartyVeggiesSkill.Code, heartyVeggiesSkill);

            var theMillerSkill = new TheMillerSkill();
            this.Skills.Add(theMillerSkill.Code, theMillerSkill);

            var gentleTouchSkill = new GentleTouchSkill();
            this.Skills.Add(gentleTouchSkill.Code, gentleTouchSkill);

            var groveTendingSkill = new GroveTendingSkill();
            this.Skills.Add(groveTendingSkill.Code, groveTendingSkill);

            var toTheBoneSkill = new ToTheBoneSkill();
            this.Skills.Add(toTheBoneSkill.Code, toTheBoneSkill);

            var macroBrewerySkill = new MacroBrewerySkill();
            this.Skills.Add(macroBrewerySkill.Code, macroBrewerySkill);

            var itAintEasySkill = new ItAintEasySkill();
            this.Skills.Add(itAintEasySkill.Code, itAintEasySkill);

            var rationingSkill = new RationingSkill();
            this.Skills.Add(rationingSkill.Code, rationingSkill);

            var brewMasterSkill = new BrewMasterSkill();
            this.Skills.Add(brewMasterSkill.Code, brewMasterSkill);

            var earlyBirdSkill = new EarlyBirdSkill();
            this.Skills.Add(earlyBirdSkill.Code, earlyBirdSkill);

            var foolsFlaxSkill = new FoolsFlaxSkill();
            this.Skills.Add(foolsFlaxSkill.Code, foolsFlaxSkill);

            var greenerThanGreenSkill = new GreenerThanGreenSkill();
            this.Skills.Add(greenerThanGreenSkill.Code, greenerThanGreenSkill);

            var miracleoflife = new MiracleOfLifeSkill();
            this.Skills.Add(miracleoflife.Code, miracleoflife);

            this.ExclusiveGroupLimits.Add("yieldskills", 2);
        }

        public override string GetDisplayName(int level)
        {
            return Lang.Get("destariamasteries:homesteader-mastery-name");
        }

        public override string GetDescription(int level) 
        {
            return Lang.Get("destariamasteries:homesteader-mastery-desc");
        }
    }

    public class SubsistanceSkill : Skill
    {
        public override string Code => "Subsistance";
        public override int MaxLevel => 5;
        public override int Column => 2;
        public override EnumSkillType SkillType => EnumSkillType.Passive;
        public override int RequiredMasteryLevel => 1;
        public SubsistanceSkill()
        {
            Attributes["PassiveStats"] = new Dictionary<string, StatConfiguration>
            {
                ["fruitHarvestMultiplier"] = new("Linear", new() { ["BaseValue"] = 0.05f }),
                ["grainHarvestMultiplier"] = new("Linear", new() { ["BaseValue"] = 0.05f }),
                ["vegetableHarvestMultiplier"] = new("Linear", new() { ["BaseValue"] = 0.05f }),
                ["forageDropRate"] = new("Linear", new() { ["BaseValue"] = 0.05f }),
                ["meatDropRate"] = new("Linear", new() { ["BaseValue"] = 0.02f }),
                ["fishFlayMultiplier"] = new("Linear", new() { ["BaseValue"] = 0.25f })
            };
        }

        public override string GetDisplayName(int level)
        {
            return Lang.Get("destariamasteries:homesteader-subsistence-name");
        }

        public override string GetDescription(int level) =>
            Lang.Get("destariamasteries:homesteader-subsistence-desc",
                this.GetScaledPercentArgs(level, "fruitHarvestMultiplier", "meatDropRate"));
    }

    public class AmberCombsSkill : Skill
    {
        public override string Code => "AmberCombs";
        public override int MaxLevel => 1;
        public override int Column => 0;
        public override EnumSkillType SkillType => EnumSkillType.Passive;
        public override int RequiredMasteryLevel => 6;
        public AmberCombsSkill()
        {

        }

        public override string GetDisplayName(int level)
        {
            return Lang.Get("destariamasteries:homesteader-ambercombs-name");
        }

        public override string GetDescription(int level)
        {
            return Lang.Get("destariamasteries:homesteader-ambercombs-desc");
        }
    }

    public class EmeraldBoughsSkill : Skill
    {
        public override string Code => "EmeraldBoughs";
        public override int MaxLevel => 1;
        public override int Column => 1;
        public override EnumSkillType SkillType => EnumSkillType.Passive;
        public override int RequiredMasteryLevel => 6;
        public EmeraldBoughsSkill()
        {
            var fruitTreeCuttingMult = new Dictionary<string, object> { { "BaseValue", 1.0f } };
            var saplingMult = new Dictionary<string, object> { { "BaseValue", 1.0f } };
            var passiveStats = new Dictionary<string, StatConfiguration> { { "fruitTreeCuttingDropRate", new StatConfiguration("Linear", fruitTreeCuttingMult) } };
            passiveStats.Add("saplingDropRate", new StatConfiguration("Linear", saplingMult));
            Attributes.Add("PassiveStats", passiveStats);
        }

        public override string GetDisplayName(int level)
        {
            return Lang.Get("destariamasteries:homesteader-emeraldboughs-name");
        }

        public override string GetDescription(int level)
        {
            return Lang.Get("destariamasteries:homesteader-emeraldboughs-desc");
        }
    }

    public class GoldenFieldsSkill : Skill
    {
        public override string Code => "GoldenFields";
        public override int MaxLevel => 1;
        public override int Column => 2;
        public override EnumSkillType SkillType => EnumSkillType.Passive;
        public override int RequiredMasteryLevel => 6;
        public GoldenFieldsSkill()
        {
            var grainMult = new Dictionary<string, object> { { "BaseValue", 0.25f } };
            var vegetableBoost = new Dictionary<string, object> { { "BaseValue", 0.25f } };
            var passiveStats = new Dictionary<string, StatConfiguration> { { "grainHarvestMultiplier", new StatConfiguration("Linear", grainMult) } };
            passiveStats.Add("vegetableHarvestMultiplier", new StatConfiguration("Linear", vegetableBoost));

            Attributes.Add("PassiveStats", passiveStats);
        }

        public override string GetDisplayName(int level)
        {
            return Lang.Get("destariamasteries:homesteader-goldenfields-name");
        }

        public override string GetDescription(int level)
        {
            return Lang.Get("destariamasteries:homesteader-goldenfields-desc",
                this.GetScaledPercentArgs(level, "grainHarvestMultiplier"));
        }
    }

    public class DeepBlueSkill : Skill
    {
        public override string Code => "DeepBlue";
        public override int MaxLevel => 1;
        public override int Column => 3;
        public override EnumSkillType SkillType => EnumSkillType.Passive;
        public override int RequiredMasteryLevel => 6;
        public DeepBlueSkill()
        {
            var fishMult = new Dictionary<string, object> { { "BaseValue", 0.25f } };
            var passiveStats = new Dictionary<string, StatConfiguration> { { "fishFlayMultiplier", new StatConfiguration("Linear", fishMult) } };
            Attributes.Add("PassiveStats", passiveStats);
        }

        public override string GetDisplayName(int level)
        {
            return Lang.Get("destariamasteries:homesteader-deepblue-name");
        }

        public override string GetDescription(int level)
        {
            return Lang.Get("destariamasteries:homesteader-deepblue-desc",
                this.GetScaledPercentArgs(level, "fishFlayMultiplier"));
        }
    }

    public class WhispersInRed : Skill
    {
        public override string Code => "WhispersInRed";
        public override int MaxLevel => 1;
        public override int Column => 4;
        public override EnumSkillType SkillType => EnumSkillType.Passive;
        public override int RequiredMasteryLevel => 6;
        public WhispersInRed()
        {
            var passiveStats = new Dictionary<string, StatConfiguration> { { "animalLootDropRate", new StatConfiguration("Linear", new Dictionary<string, object> { { "BaseValue", 0.25f } }) } };
            Attributes.Add("PassiveStats", passiveStats);
        }

        public override string GetDisplayName(int level)
        {
            return Lang.Get("destariamasteries:homesteader-whispersinred-name");
        }

        public override string GetDescription(int level)
        {
            return Lang.Get("destariamasteries:homesteader-whispersinred-desc",
                this.GetScaledPercentArgs(level, "animalLootDropRate"));
        }
    }

    public class SelfsufficientSkill : Skill
    {
        public override string Code => "Selfsufficient";
        public override int MaxLevel => 3;
        public override int Column => 2;
        public override EnumSkillType SkillType => EnumSkillType.Passive;
        public override int RequiredMasteryLevel => 8;
        public SelfsufficientSkill()
        {
            var passiveStats = new Dictionary<string, StatConfiguration> { { "hungerrate ", new StatConfiguration("Array", new Dictionary<string, object> { { "Values", new float[] { 0.0f, -0.1f, -0.1f } } } ) } };
            Attributes.Add("PassiveStats", passiveStats);
        }

        public override string GetDisplayName(int level)
        {
            return Lang.Get("destariamasteries:homesteader-selfsufficient-name");
        }

        public override string GetDescription(int level)
        {
            string desc = Lang.Get("destariamasteries:homesteader-selfsufficient-desc-levelone");
            if (level >= 2)
            {
                desc += Lang.Get("destariamasteries:homesteader-selfsufficient-desc-leveltwo", this.GetScaledPercentArgs(level, "hungerrate"));
            }
            if (level >= 3)
            {
                desc += Lang.Get("destariamasteries:homesteader-selfsufficient-desc-levelthree");
            }
            return desc;
        }
    }

    public class BitFruitySkill : Skill
    {
        public override string Code => "BitFruity";
        public override int MaxLevel => 5;
        public override int Column => 0;
        public override EnumSkillType SkillType => EnumSkillType.Passive;
        public override int RequiredMasteryLevel => 11;
        public BitFruitySkill()
        {
            var passiveStats = new Dictionary<string, StatConfiguration> { { "fruitHarvestMultiplier", new StatConfiguration("Linear", new Dictionary<string, object> { { "BaseValue", 0.05f } }) } };
            passiveStats.Add("forageDropRate", new StatConfiguration("Linear", new Dictionary<string, object> { { "BaseValue", 0.05f } }));
            Attributes.Add("PassiveStats", passiveStats);
            var activeParameters = new Dictionary<string, object> { { "Values", new float[] { 0.05f, 0.10f, 0.15f, 0.20f, 0.25f } } };
            var activestats = new Dictionary<string, StatConfiguration> { { "walkspeed", new StatConfiguration("Array", activeParameters) } };
            Attributes.Add("ActiveStats", activestats);
        }

        public override string GetDisplayName(int level)
        {
            return Lang.Get("destariamasteries:homesteader-bitfruity-name");
        }

        public override string GetDescription(int level)
        {
            string desc = Lang.Get("destariamasteries:homesteader-bitfruity-desc",
                this.GetScaledPercentArgs(level, "fruitHarvestMultiplier"));
            if (level == 5)
            {
                desc += Lang.Get("destariamasteries:homesteader-bitfruity-desc-level5",
                    this.GetScaledPercentArgs(level, "walkspeed"));
            }
            return desc;
        }
    }

    public class HeartyVeggiesSkill : Skill
    {
        public override string Code => "HeartyVeggies";
        public override int MaxLevel => 5;
        public override int Column => 1;
        public override EnumSkillType SkillType => EnumSkillType.Passive;
        public override int RequiredMasteryLevel => 11;
        public HeartyVeggiesSkill()
        {
            var passiveStats = new Dictionary<string, StatConfiguration> { { "vegetableHarvestMultiplier", new StatConfiguration("Linear", new Dictionary<string, object> { { "BaseValue", 0.05f } }) } };
            passiveStats.Add("vegetableSaturation", new StatConfiguration("Array", new Dictionary<string, object> { { "Values", new float[] { 0.0f, 0.0f, 0.0f, 0.0f, 1.00f } } }));
            Attributes.Add("PassiveStats", passiveStats);
        }

        public override string GetDisplayName(int level)
        {
            return Lang.Get("destariamasteries:homesteader-heartyveggies-name");
        }

        public override string GetDescription(int level)
        {
            string desc = Lang.Get("destariamasteries:homesteader-heartyveggies-desc",
                this.GetScaledPercentArgs(level, "vegetableHarvestMultiplier"));
            if (level == 5)
            {
                desc += Lang.Get("destariamasteries:homesteader-heartyveggies-desc-level5",
                    this.GetScaledPercentArgs(level, "vegetableSaturation"));
            }
            return desc;
        }
    }

    public class TheMillerSkill : Skill
    {
        public override string Code => "TheMiller";
        public override int MaxLevel => 5;
        public override int Column => 2;
        public override EnumSkillType SkillType => EnumSkillType.Passive;
        public override int RequiredMasteryLevel => 11;
        public TheMillerSkill()
        {
            var passiveStats = new Dictionary<string, StatConfiguration> { { "forageDropRate", new StatConfiguration("Linear", new Dictionary<string, object> { { "BaseValue", 0.05f } }) } };
            passiveStats.Add("grassHarvestMultiplier", new StatConfiguration("Linear", new Dictionary<string, object> { { "BaseValue", 0.05f } }));
            Attributes.Add("PassiveStats", passiveStats);
        }

        public override string GetDisplayName(int level)
        {
            return Lang.Get("destariamasteries:homesteader-themiller-name");
        }

        public override string GetDescription(int level)
        {
            string desc = Lang.Get("destariamasteries:homesteader-themiller-desc",
                this.GetScaledPercentArgs(level, "forageDropRate", "grassHarvestMultiplier"));
            if (level == 5)
            {
                desc += Lang.Get("destariamasteries:homesteader-themiller-desc-level5",
                    this.GetScaledPercentArgs(level, "quernSpeed"));
            }
            return desc;
        }
    }

    public class GroveTendingSkill : Skill
    {
        public override string Code => "GroveTending";
        public override string Ability => "GroveTending";
        public override int MaxLevel => 5;
        public override int Column => 2;
        public override EnumSkillType SkillType => EnumSkillType.Active;
        public override int RequiredMasteryLevel => 11;
        public GroveTendingSkill()
        {
            var passiveStats = new Dictionary<string, StatConfiguration> { { "forageDropRate", new StatConfiguration("Linear", new Dictionary<string, object> { { "BaseValue", 0.02f } }) } };
            Attributes.Add("PassiveStats", passiveStats);

            var activeParameters = new Dictionary<string, object> { { "Values", new float[] { 0.1f, 0.2f, 0.3f, 0.5f, 1.0f } } };
            var activestats = new Dictionary<string, StatConfiguration> { { "cuttingReviveChance", new StatConfiguration("Array", activeParameters) } };
            Attributes.Add("ActiveStats", activestats);
        }

        public override string GetDisplayName(int level)
        {
            return Lang.Get("destariamasteries:homesteader-grovetending-name");
        }

        public override string GetDescription(int level)
        {
            return Lang.Get("destariamasteries:homesteader-grovetending-desc", 
                this.GetScaledPercentActiveArgs(level, "cuttingReviveChance"),
                this.GetScaledPercentArgs(level, "forageDropRate"));
        }
    }
    public class GentleTouchSkill : Skill
    {
        public override string Code => "GentleTouch";
        public override string Ability => "GentleTouch";
        public override float Cooldown => 86400;
        public override int MaxLevel => 5;
        public override int Column => 3;
        public override EnumSkillType SkillType => EnumSkillType.Active;
        public override int RequiredMasteryLevel => 16;
        public GentleTouchSkill()
        {
            var activeParameters = new Dictionary<string, object> { { "Values", new float[] { 0.1f, 0.2f, 0.3f, 0.5f, 0.8f } } };
            var activestats = new Dictionary<string, StatConfiguration> { { "calmChance", new StatConfiguration("Array", activeParameters) } };
            Attributes.Add("ActiveStats", activestats);
        }

        public override string GetDisplayName(int level)
        {
            return Lang.Get("destariamasteries:homesteader-gentletouch-name");
        }

        public override string GetDescription(int level)
        {
            return Lang.Get("destariamasteries:homesteader-gentletouch-desc",
                this.GetScaledPercentActiveArgs(level, "calmChance"));
        }
    }

    public class ToTheBoneSkill : Skill
    {
        public override string Code => "ToTheBone";
        public override string Ability => "ToTheBone";
        public override int MaxLevel => 5;
        public override int Column => 4;
        public override EnumSkillType SkillType => EnumSkillType.Active;
        public override int RequiredMasteryLevel => 16;
        public ToTheBoneSkill()
        {
        }

        public override string GetDisplayName(int level)
        {
            return Lang.Get("destariamasteries:homesteader-tothebone-name");
        }

        public override string GetDescription(int level)
        {
            return Lang.Get("destariamasteries:homesteader-tothebone-desc", level);
        }
    }

    public class MacroBrewerySkill : Skill
    {
        public override string Code => "MacroBrewery";
        public override int MaxLevel => 5;
        public override int Column => 1;
        public override EnumSkillType SkillType => EnumSkillType.Passive;
        public override int RequiredMasteryLevel => 21;
        public MacroBrewerySkill()
        {
            var passiveStats = new Dictionary<string, StatConfiguration> { { "fruitPressSaveChance", new StatConfiguration("Array", new Dictionary<string, object> { { "Values", new float[] { 0.05f, 0.1f, 0.15f, 0.2f, 0.33f } } }) } };
            Attributes.Add("PassiveStats", passiveStats);
        }

        public override string GetDisplayName(int level)
        {
            return Lang.Get("destariamasteries:homesteader-macrobrewery-name");
        }

        public override string GetDescription(int level)
        {
            return Lang.Get("destariamasteries:homesteader-macrobrewery-desc",
                this.GetScaledPercentArgs(level, "fruitPressSaveChance"));
        }
    }

    public class ItAintEasySkill : Skill
    {
        public override string Code => "ItAintEasy";
        public override int MaxLevel => 1;
        public override int Column => 2;
        public override EnumSkillType SkillType => EnumSkillType.Passive;
        public override int RequiredMasteryLevel => 21;
        public ItAintEasySkill()
        {

        }

        public override string GetDisplayName(int level)
        {
            return Lang.Get("destariamasteries:homesteader-itainteasy-name");
        }

        public override string GetDescription(int level)
        {
            return Lang.Get("destariamasteries:homesteader-itainteasy-desc");
        }
    }

    public class RationingSkill : Skill
    {
        public override string Code => "Rationing";
        public override int MaxLevel => 5;
        public override int Column => 3;
        public override EnumSkillType SkillType => EnumSkillType.Passive;
        public override int RequiredMasteryLevel => 21;
        public RationingSkill()
        {
            var passiveStats = new Dictionary<string, StatConfiguration> { { "hungerrate ", new StatConfiguration("Linear", new Dictionary<string, object> { { "BaseValue", 0.02f } }) } };
            passiveStats.Add("saveSeedChance", new StatConfiguration("Array", new Dictionary<string, object> { { "Values", new float[] { 0.03f, 0.06f, 0.09f, 0.12f, 0.2f } } }));
            passiveStats.Add("saveFertilizerChance", new StatConfiguration("Array", new Dictionary<string, object> { { "Values", new float[] { 0.03f, 0.06f, 0.09f, 0.12f, 0.2f } } }));
            Attributes.Add("PassiveStats", passiveStats);
        }

        public override string GetDisplayName(int level)
        {
            return Lang.Get("destariamasteries:homesteader-rationing-name");
        }

        public override string GetDescription(int level)
        {
            return Lang.Get("destariamasteries:homesteader-rationing-desc",
                this.GetScaledPercentArgs(level, "saveFertilizerChance", "hungerrate"));
        }
    }

    public class BrewMasterSkill : Skill
    {
        public override string Code => "BrewMaster";
        public override string Ability => "BrewMaster";
        public override float Cooldown => 86400;
        public override int MaxLevel => 5;
        public override int Column => 1;
        public override EnumSkillType SkillType => EnumSkillType.Active;
        public override int RequiredMasteryLevel => 26;
        public BrewMasterSkill()
        {
            var passiveStats = new Dictionary<string, StatConfiguration> { { "damageResistanceWhenDrunk", new StatConfiguration("Array", new Dictionary<string, object> { { "Values", new float[] { -0.04f, -0.08f, -0.12f, -0.2f, -0.3f } } }) } };
            Attributes.Add("PassiveStats", passiveStats);
        }

        public override string GetDisplayName(int level)
        {
            return Lang.Get("destariamasteries:homesteader-brewmaster-name");
        }

        public override string GetDescription(int level)
        {
            return Lang.Get("destariamasteries:homesteader-brewmaster-desc",
                this.GetScaledPercentActiveArgs(level, "damageResistanceWhenDrunk"));
        }
    }

    public class EarlyBirdSkill : Skill
    {
        public override string Code => "EarlyBird";
        public override int MaxLevel => 1;
        public override int Column => 2;
        public override EnumSkillType SkillType => EnumSkillType.Passive;
        public override int RequiredMasteryLevel => 26;
        public EarlyBirdSkill()
        {

        }

        public override string GetDisplayName(int level)
        {
            return Lang.Get("destariamasteries:homesteader-earlybird-name");
        }

        public override string GetDescription(int level)
        {
            return Lang.Get("destariamasteries:homesteader-earlybird-desc");
        }
    }

    public class FoolsFlaxSkill : Skill
    {
        public override string Code => "FoolsFlax";
        public override int MaxLevel => 5;
        public override int Column => 2;
        public override EnumSkillType SkillType => EnumSkillType.Passive;
        public override int RequiredMasteryLevel => 30;
        public FoolsFlaxSkill()
        {
            var passiveStats = new Dictionary<string, StatConfiguration> { { "flaxFibreDropRate", new StatConfiguration("Linear", new Dictionary<string, object> { { "BaseValue", 0.1f } }) } };
            Attributes.Add("PassiveStats", passiveStats);
        }

        public override string GetDisplayName(int level)
        {
            return Lang.Get("destariamasteries:homesteader-foolsflax-name");
        }

        public override string GetDescription(int level)
        {
            return Lang.Get("destariamasteries:homesteader-foolsflax-desc",
                this.GetScaledPercentArgs(level, "flaxFibreDropRate"));
        }
    }

    public class GreenerThanGreenSkill : Skill 
    {
        public override string Code => "GreenerThanGreen";
        public override string Ability => "GreenerThanGreen";
        public override float Cooldown => 604800;
        public override int MaxLevel => 1;
        public override int Column => 1;
        public override EnumSkillType SkillType => EnumSkillType.Active;
        public override int RequiredMasteryLevel => 30;
        public GreenerThanGreenSkill()
        {
        }

        public override string GetDisplayName(int level)
        {
            return Lang.Get("destariamasteries:homesteader-greenerthangreen-name");
        }

        public override string GetDescription(int level)
        {
            return Lang.Get("destariamasteries:homesteader-greenerthangreen-desc");
        }
    }

    public class MiracleOfLifeSkill : Skill
    {
        public override string Code => "MiracleOfLife";
        public override string Ability => "MiracleOfLife";
        public override float Cooldown => 86400;
        public override int MaxLevel => 1;
        public override int Column => 3;
        public override EnumSkillType SkillType => EnumSkillType.Active;
        public override int RequiredMasteryLevel => 30;
        public MiracleOfLifeSkill()
        {
            LevelRequirements = new Dictionary<int, List<SkillPrerequisite>>
            {
                {1, new List<SkillPrerequisite> { new SkillPrerequisite { SkillCode = "GentleTouch", MinimumLevel = 1 } } }
            };
        }

        public override string GetDisplayName(int level)
        {
            return Lang.Get("destariamasteries:homesteader-miracleoflife-name");
        }

        public override string GetDescription(int level)
        {
            return Lang.Get("destariamasteries:homesteader-miracleoflife-desc");
        }
    }
}
