using MasteryLibrary.src.Core.Masteries.Data;
using MasteryLibrary.src.Utilities;
using System.Collections.Generic;
using Vintagestory.API.Config;

namespace DestariaMasteries.src.Healing
{
    public class HealingMastery : Mastery
    {
        public override string Code => "Healing";
        public override int MaxLevel => 30;

        public HealingMastery()
        {
            var thickSkinned = new ThickSkinnedSkill();
            Skills.Add(thickSkinned.Code, thickSkinned);

            var soulRecovery = new SoulRecoverySkill();
            Skills.Add(soulRecovery.Code, soulRecovery);

            var soulTouch = new SoulTouchSkill();
            Skills.Add(soulTouch.Code, soulTouch);

            var herbalRemedies = new HerbalRemedies();
            Skills.Add(herbalRemedies.Code, herbalRemedies);

            var shockingFingers = new ShockingFingersSkill();
            Skills.Add(shockingFingers.Code, shockingFingers);

        }

        public override string GetDisplayName(int level)
        {
            return Lang.Get("destariamasteries:healing-mastery-name");
        }

        public override string GetDescription(int level)
        {
            return Lang.Get("destariamasteries:healing-mastery-desc");
        }
    }

    public class ThickSkinnedSkill : Skill
    {
        public override string Code => "ThickSkinned";
        public override int MaxLevel => 5;
        public override int Column => 0;
        public override EnumSkillType SkillType => EnumSkillType.Passive;

        public ThickSkinnedSkill()
        {
            var parameters = new Dictionary<string, object> { { "BaseValue", 0.05f } };
            var passiveStats = new Dictionary<string, StatConfiguration>
            {
                { "healingeffectivness", new StatConfiguration("Linear", parameters) },
                { "healingItemUseSpeed", new StatConfiguration("Linear", parameters) }
            };

            Attributes.Add("PassiveStats", passiveStats);
        }

        public override string GetDisplayName(int level)
        {
            return Lang.Get("destariamasteries:healing-thickskinned-name");
        }

        public override string GetDescription(int level)
        {
            var passiveStats = GetValueSafe<Dictionary<string, StatConfiguration>>("PassiveStats");
            float healingEffectiveness = StatScalingUtil.GetScaledValue(
                passiveStats?.GetValueOrDefault("healingeffectivness"), level);

            return Lang.Get("destariamasteries:healing-thickskinned-desc", healingEffectiveness * 100);
        }
    }

    public class SoulRecoverySkill : Skill
    {
        public override string Code => "SoulRecovery";
        public override string IconPath => "destariamasteries:textures/masteries/healing/soulrecovery.png";
        public override int MaxLevel => 5;
        public override int Column => 1;
        public override string Ability => "SoulRecovery";
        public override float Cooldown => 10f;
        public override EnumSkillType SkillType => EnumSkillType.Active;

        public SoulRecoverySkill()
        {
            var parameters = new Dictionary<string, object>
            {
                { "Values", new float[] { 0.10f, 0.15f, 0.20f, 0.25f, 0.30f } }
            };

            Attributes.Add("MissingHealthHealPercent", new StatConfiguration("Array", parameters));
        }

        public override string GetDisplayName(int level)
        {
            return Lang.Get("destariamasteries:healing-soulrecovery-name");
        }

        public override string GetDescription(int level)
        {
            float healPercent = StatScalingUtil.GetScaledValue(
                GetValueSafe<StatConfiguration>("MissingHealthHealPercent"), level);

            return Lang.Get("destariamasteries:healing-soulrecovery-desc", healPercent * 100, Cooldown / 60f);
        }
    }

    public class SoulTouchSkill : Skill
    {
        public override string Code => "SoulTouch";
        public override int MaxLevel => 5;
        public override int Column => 1;
        public override int RequiredMasteryLevel => 8;
        public override EnumSkillType SkillType => EnumSkillType.Passive;

        public SoulTouchSkill()
        {
            LevelRequirements = new Dictionary<int, List<SkillPrerequisite>>
            {
                { 1, new List<SkillPrerequisite> { new SkillPrerequisite { SkillCode = "SoulRecovery", MinimumLevel = 1 } } },
                { 2, new List<SkillPrerequisite> { new SkillPrerequisite { SkillCode = "SoulRecovery", MinimumLevel = 2 } } },
                { 3, new List<SkillPrerequisite> { new SkillPrerequisite { SkillCode = "SoulRecovery", MinimumLevel = 3 } } },
                { 4, new List<SkillPrerequisite> { new SkillPrerequisite { SkillCode = "SoulRecovery", MinimumLevel = 4 } } },
                { 5, new List<SkillPrerequisite> { new SkillPrerequisite { SkillCode = "SoulRecovery", MinimumLevel = 5 } } }
            };

            var parameters = new Dictionary<string, object>
            {
                { "Values", new float[] { 0.20f, 0.30f, 0.40f, 0.50f, 0.60f } }
            };

            Attributes.Add("SharedHealPercent", new StatConfiguration("Array", parameters));
        }

        public override string GetDisplayName(int level)
        {
            return Lang.Get("destariamasteries:healing-soultouch-name");
        }

        public override string GetDescription(int level)
        {
            float sharedHealPercent = StatScalingUtil.GetScaledValue(
                GetValueSafe<StatConfiguration>("SharedHealPercent"), level);

            return Lang.Get("destariamasteries:healing-soultouch-desc", sharedHealPercent * 100);
        }
    }

    public class HerbalRemedies : Skill
    {
        public override string Code => "HerbalRemedies";
        public override string IconPath => "destariamasteries:textures/masteries/healing/herbalremedies.png";

        public override int Column => 2;

        public override EnumSkillType SkillType => EnumSkillType.Passive;

        public override string GetDisplayName(int level)

        {
            return Lang.Get("destariamasteries:healing-herbalremedies-name");
        }

        public override string GetDescription(int level)
        {
            return Lang.Get("destariamasteries:healing-herbalremedies-desc");
        }
    }

    public class ShockingFingersSkill : Skill
    {
        public override string Code => "ShockingFingers";
        public override int MaxLevel => 1;
        public override int Column => 3;
        public override int RequiredMasteryLevel => 15;
        public override string Ability => "ShockingFingers";
        public override float Cooldown => 600f;
        public override EnumSkillType SkillType => EnumSkillType.Active;

        public override string GetDisplayName(int level)
        {
            return Lang.Get("destariamasteries:healing-shockingfingers-name");
        }

        public override string GetDescription(int level)
        {
            return Lang.Get("destariamasteries:healing-shockingfingers-desc", Cooldown / 60f);
        }
    }
}
