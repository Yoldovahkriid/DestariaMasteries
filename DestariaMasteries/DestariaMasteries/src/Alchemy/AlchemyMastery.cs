using MasteryLibrary.src.Core.Masteries.Data;
using MasteryLibrary.src.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Config;

namespace DestariaMasteries.src.Alchemy
{
    public class AlchemyMastery : Mastery
    {
        public override string Code => "Alchemy";
        public override int MaxLevel => 30;
        public AlchemyMastery()
        {
            var basicdecoctions = new BasicConcoctionsSkill();
            this.Skills.Add(basicdecoctions.Code, basicdecoctions);

            var fungalnourishment = new FungalNourishmentSkill();
            this.Skills.Add(fungalnourishment.Code, fungalnourishment);

            var toxictransmutation = new ToxicTransmutationSkill();
            this.Skills.Add(toxictransmutation.Code, toxictransmutation);

            var fungalfingers = new FungalFingersSkill();
            this.Skills.Add(fungalfingers.Code, fungalfingers);

            var virulentreaction = new VirulentReactionSkill();
            this.Skills.Add(virulentreaction.Code, virulentreaction);

            var potentdecoctions = new PotentDecoctionsSkill();
            this.Skills.Add(potentdecoctions.Code, potentdecoctions);

            var dazzleblast = new DazzleBlastSkill();
            this.Skills.Add(dazzleblast.Code, dazzleblast);

            var smokeveil = new PoisonCloudSkill();
            this.Skills.Add(smokeveil.Code, smokeveil);

            var concussiveblast = new ConcussiveBlastSkill();
            this.Skills.Add(concussiveblast.Code, concussiveblast);

            var primedtoxin = new PrimedToxinSkill();
            this.Skills.Add(primedtoxin.Code, primedtoxin);

            var granddetonations = new GrandDetonationsSkill();
            this.Skills.Add(granddetonations.Code, granddetonations);

            var toxicreserves = new ToxicReservesSkill();
            this.Skills.Add(toxicreserves.Code, toxicreserves);

            var costlymistake = new CostlyMistakeSkill();
            this.Skills.Add(costlymistake.Code, costlymistake);

            var catalyticsurge = new CatalyticSurgeSkill();
            this.Skills.Add(catalyticsurge.Code, catalyticsurge);
        }

        public override string GetDisplayName(int level)
        {
            return Lang.Get("destariamasteries:alchemy-mastery-name");
        }
        public override string GetDescription(int level)
        {
            return Lang.Get("destariamasteries:alchemy-mastery-desc");
        }
    }

    public class BasicConcoctionsSkill : Skill
    {
        public override string Code => "BasicConcoctions";
        public override string IconPath => "destariamasteries:textures/masteries/alchemy/basicconcoctions.png";

        public override int Column => 0;

        public override EnumSkillType SkillType => EnumSkillType.Passive;

        public override string GetDisplayName(int level)
        {
            return Lang.Get("destariamasteries:alchemy-basicconcoctions-name");
        }

        public override string GetDescription(int level)
        {
            return Lang.Get("destariamasteries:alchemy-basicconcoctions-desc");
        }
    }

    public class FungalNourishmentSkill : Skill
    {
        public override string Code => "FungalNourishment";
        public override string IconPath => "destariamasteries:textures/masteries/alchemy/fungalnourishment.png";
        public override int MaxLevel => 5;
        public override float Duration => 10f;
        public override int Column => 3;

        public override EnumSkillType SkillType => EnumSkillType.Passive;

        public FungalNourishmentSkill()
        {
            var parameters = new Dictionary<string, object> { { "Values", new float[] { 25, 50, 75, 100, 125 } } };
            var passivestats = new Dictionary<string, StatConfiguration> { { "mushroomsSaturation", new StatConfiguration("Array", parameters) } };
            var parameters2 = new Dictionary<string, object> { { "Values", new float[] { 0.05f, 0.10f, 0.15f, 0.20f, 0.25f } } };
            var activestats = new Dictionary<string, StatConfiguration> { { "healingeffectivness", new StatConfiguration("Array", parameters2) } };
            Attributes.Add("PassiveStats", passivestats);
            Attributes.Add("ActiveStats", activestats);
        }

        public override string GetDisplayName(int level)
        {
            return Lang.Get("destariamasteries:alchemy-fungalnourishment-name");
        }

        public override string GetDescription(int level)
        {
            var passiveStats = GetValueSafe<Dictionary<string, StatConfiguration>>("PassiveStats");
            var activeStats = GetValueSafe<Dictionary<string, StatConfiguration>>("ActiveStats");

            float saturation_bonus = StatScalingUtil.GetScaledValue(
                passiveStats?.GetValueOrDefault("mushroomsSaturation"), level);
            float heal_bonus_percent = StatScalingUtil.GetScaledValue(
                activeStats?.GetValueOrDefault("healingeffectivness"), level);

            return Lang.Get("destariamasteries:alchemy-fungalnourishment-desc",
                saturation_bonus, heal_bonus_percent * 100);
        }
    }

    public class ToxicTransmutationSkill : Skill
    {
        public override string Code => "ToxicTransmutation";
        public override string IconPath => "destariamasteries:textures/masteries/alchemy/toxictransmutation.png";
        public override int Column => 6;
        public override int MaxLevel => 5;

        public override EnumSkillType SkillType => EnumSkillType.Passive;
        public ToxicTransmutationSkill()
        {
            var parameters = new Dictionary<string, object> { { "BaseValue", -0.20f } };
            var passivestats = new Dictionary<string, StatConfiguration> { { "mushroomDamageMult", new StatConfiguration("Linear", parameters) } };
            Attributes.Add("PassiveStats", passivestats);
        }

        public override string GetDisplayName(int level)
        {
            return Lang.Get("destariamasteries:alchemy-toxictransmutation-name");
        }

        public override string GetDescription(int level)
        {
            var passiveStats = GetValueSafe<Dictionary<string, StatConfiguration>>("PassiveStats");
            float damage_mult = StatScalingUtil.GetScaledValue(
                passiveStats?.GetValueOrDefault("mushroomDamageMult"), level);
            return Lang.Get("destariamasteries:alchemy-toxictransmutation-desc", damage_mult * 100);
        }
    }

    public class FungalFingersSkill : Skill
    {
        public override string Code => "FungalFingers";
        public override string IconPath => "destariamasteries:textures/masteries/alchemy/fungalfingers.png";

        public override int Column => 0;
        public override int RequiredMasteryLevel => 8;

        public override EnumSkillType SkillType => EnumSkillType.Passive;

        public FungalFingersSkill()
        {
            LevelRequirements = new Dictionary<int, List<SkillPrerequisite>>
            {
                {1, new List<SkillPrerequisite> { new SkillPrerequisite { SkillCode = "BasicConcoctions", MinimumLevel = 1 } } }
            };
        }

        public override string GetDisplayName(int level)
        {
            return Lang.Get("destariamasteries:alchemy-fungalfingers-name");
        }

        public override string GetDescription(int level)
        {
            return Lang.Get("destariamasteries:alchemy-fungalfingers-desc");
        }
    }

    public class VirulentReactionSkill : Skill
    {
        public override string Code => "VirulentReaction";
        public override string IconPath => "destariamasteries:textures/masteries/alchemy/virulentreaction.png";

        public override int Column => 1;
        public override int MaxLevel => 5;
        public override int RequiredMasteryLevel => 8;

        public override EnumSkillType SkillType => EnumSkillType.Passive;
        public VirulentReactionSkill()
        {
            var parameters = new Dictionary<string, object> { { "BaseValue", 0.1f } };
            var passivestats = new Dictionary<string, StatConfiguration> { { "poisonDamageMul", new StatConfiguration("Linear", parameters) } };
            Attributes.Add("PassiveStats", passivestats);
        }

        public override string GetDisplayName(int level)
        {
            return Lang.Get("destariamasteries:alchemy-virulentreaction-name");
        }

        public override string GetDescription(int level)
        {
            var passiveStats = GetValueSafe<Dictionary<string, StatConfiguration>>("PassiveStats");
            float poison_mult = StatScalingUtil.GetScaledValue(
                passiveStats?.GetValueOrDefault("poisonDamageMul"), level);
            return Lang.Get("destariamasteries:alchemy-virulentreaction-desc", poison_mult * 100);
        }
    }

    public class PotentDecoctionsSkill : Skill
    {
        public override string Code => "PotentDecoctions";
        public override string IconPath => "destariamasteries:textures/masteries/alchemy/potentconcoctions.png";

        public override int Column => 0;
        public override int RequiredMasteryLevel => 15;

        public override EnumSkillType SkillType => EnumSkillType.Passive;

        public PotentDecoctionsSkill()
        {
            LevelRequirements = new Dictionary<int, List<SkillPrerequisite>>
            {
                {1, new List<SkillPrerequisite> { new SkillPrerequisite { SkillCode = "FungalFingers", MinimumLevel = 1 } } }
            };
        }

        public override string GetDisplayName(int level)
        {
            return Lang.Get("destariamasteries:alchemy-potentdecoctions-name");
        }

        public override string GetDescription(int level)
        {
            return Lang.Get("destariamasteries:alchemy-potentdecoctions-desc");
        }
    }

    public class DazzleBlastSkill : Skill
    {
        public override string Code => "DazzleBlast";
        public override string IconPath => "destariamasteries:textures/masteries/alchemy/dazzleblast.png";
        public override string Ability => "DazzleBlast";
        public override float Cooldown => 120f;
        public override int MaxLevel => 5;
        public override int Column => 4;
        public override int RequiredMasteryLevel => 15;

        public override EnumSkillType SkillType => EnumSkillType.Active;

        public override string GetDisplayName(int level)
        {
            return Lang.Get("destariamasteries:alchemy-dazzleblast-name");
        }

        public override string GetDescription(int level)
        {
            return Lang.Get("destariamasteries:alchemy-dazzleblast-desc", 2 * level);
        }
    }

    public class PoisonCloudSkill : Skill
    {
        public override string Code => "PoisonCloud";
        public override string IconPath => "destariamasteries:textures/masteries/alchemy/poisoncloud.png";
        public override int MaxLevel => 5;
        public override string Ability => "PoisonCloud";
        public override float Cooldown => 0f;
        public override int Column => 2;
        public override int RequiredMasteryLevel => 23;

        public override EnumSkillType SkillType => EnumSkillType.Active;
        public override string GetDisplayName(int level)
        {
            return Lang.Get("destariamasteries:alchemy-poisoncloud-name");
        }

        public override string GetDescription(int level)
        {
            return Lang.Get("destariamasteries:alchemy-poisoncloud-desc", 5.0f + 2.0f * level);
        }
    }

    public class ConcussiveBlastSkill : Skill
    {
        public override string Code => "ConcussiveBlast";
        public override string IconPath => "destariamasteries:textures/masteries/alchemy/concussiveblast.png";
        public override int Column => 4;
        public override int MaxLevel => 5;
        public override int RequiredMasteryLevel => 23;
        public override EnumSkillType SkillType => EnumSkillType.Passive;

        public ConcussiveBlastSkill()
        {
            LevelRequirements = new Dictionary<int, List<SkillPrerequisite>>
            {
                { 1, new List<SkillPrerequisite> { new SkillPrerequisite { SkillCode = "DazzleBlast", MinimumLevel = 1 } } },
                { 2, new List<SkillPrerequisite> { new SkillPrerequisite { SkillCode = "DazzleBlast", MinimumLevel = 2 } } },
                { 3, new List<SkillPrerequisite> { new SkillPrerequisite { SkillCode = "DazzleBlast", MinimumLevel = 3 } } },
                { 4, new List<SkillPrerequisite> { new SkillPrerequisite { SkillCode = "DazzleBlast", MinimumLevel = 4 } } },
                { 5, new List<SkillPrerequisite> { new SkillPrerequisite { SkillCode = "DazzleBlast", MinimumLevel = 5 } } }
            };
        }
        public override string GetDisplayName(int level)
        {
            return Lang.Get("destariamasteries:alchemy-concussiveblast-name");
        }

        public override string GetDescription(int level)
        {
            return Lang.Get("destariamasteries:alchemy-concussiveblast-desc", 2 * level);
        }
    }

    public class PrimedToxinSkill : Skill
    {
        public override string Code => "PrimedToxin";
        public override string IconPath => "destariamasteries:textures/masteries/alchemy/primedtoxin.png";
        public override string Ability => "PrimedToxin";
        public override float Cooldown => 120f;
        public override int Column => 5;
        public override int MaxLevel => 5;
        public override int RequiredMasteryLevel => 23;

        public PrimedToxinSkill()
        {
            var parameters = new Dictionary<string, object> { { "Values", new float[] { 3, 5, 7, 9, 11 } } };
            Attributes.Add("PoisonDuration", new StatConfiguration("Array", parameters));
            Attributes.Add("PoisonDamage", 1.0f);
        }

        public override EnumSkillType SkillType => EnumSkillType.Active;

        public override string GetDisplayName(int level)
        {
            return Lang.Get("destariamasteries:alchemy-primedtoxin-name");
        }

        public override string GetDescription(int level)
        {
            float poison_duration = StatScalingUtil.GetScaledValue(
                GetValueSafe<StatConfiguration>("PoisonDuration"), level);
            return Lang.Get("destariamasteries:alchemy-primedtoxin-desc", poison_duration);
        }
    }

    public class GrandDetonationsSkill : Skill
    {
        public override string Code => "GrandDetonations";
        public override string IconPath => "destariamasteries:textures/masteries/alchemy/granddetonations.png";

        public override int Column => 0;
        public override int RequiredMasteryLevel => 30;

        public override EnumSkillType SkillType => EnumSkillType.Passive;

        public GrandDetonationsSkill()
        {
            LevelRequirements = new Dictionary<int, List<SkillPrerequisite>>
            {
                {1, new List<SkillPrerequisite> { new SkillPrerequisite { SkillCode = "PotentDecoctions", MinimumLevel = 1 } } }
            };
        }

        public override string GetDisplayName(int level)
        {
            return Lang.Get("destariamasteries:alchemy-granddetonations-name");
        }

        public override string GetDescription(int level)
        {
            return Lang.Get("destariamasteries:alchemy-granddetonations-desc");
        }
    }

    public class ToxicReservesSkill : Skill
    {
        public override string Code => "ToxicReserves";
        public override string IconPath => "destariamasteries:textures/masteries/alchemy/toxicreserves.png";

        public override int Column => 1;
        public override int MaxLevel => 5;
        public override int RequiredMasteryLevel => 30;

        public override EnumSkillType SkillType => EnumSkillType.Passive;

        public ToxicReservesSkill()
        {
            var parameters = new Dictionary<string, object> { { "Values", new float[] { 0.03f, 0.06f, 0.09f, 0.12f, 0.15f } } };
            Attributes.Add("WalkSpeedBonus", new StatConfiguration("Array", parameters));
            Attributes.Add("HungerRateReduction", -0.5f);
        }

        public override string GetDisplayName(int level)
        {
            return Lang.Get("destariamasteries:alchemy-toxicreserves-name");
        }

        public override string GetDescription(int level)
        {
            float walkspeed = StatScalingUtil.GetScaledValue(GetValueSafe<StatConfiguration>("WalkSpeedBonus"), level);
            float hungerrate = GetValueSafe<float>("HungerRateReduction");
            return Lang.Get("destariamasteries:alchemy-toxicreserves-desc", walkspeed * 100, hungerrate * 100);
        }
    }

    public class CostlyMistakeSkill : Skill
    {
        public override string Code => "CostlyMistake";
        public override string IconPath => "destariamasteries:textures/masteries/alchemy/costlymistake.png";
        public override string Ability => "CostlyMistake";
        public override float Cooldown => 0f;
        public override int Column => 3;
        public override int MaxLevel => 5;
        public override int RequiredMasteryLevel => 30;

        public override EnumSkillType SkillType => EnumSkillType.Active;

        public CostlyMistakeSkill()
        {
            var parameters = new Dictionary<string, object> { { "Values", new float[] { 3, 5, 7, 9, 11 } } };
            Attributes.Add("PoisonDuration", new StatConfiguration("Array", parameters));
            Attributes.Add("PoisonDamage", 1.0f);
        }
        public override string GetDisplayName(int level)
        {
            return Lang.Get("destariamasteries:alchemy-costlymistake-name");
        }

        public override string GetDescription(int level)
        {
            float duration = StatScalingUtil.GetScaledValue(GetValueSafe<StatConfiguration>("PoisonDuration"), level);
            return Lang.Get("destariamasteries:alchemy-costlymistake-desc", duration);
        }
    }

    public class CatalyticSurgeSkill : Skill
    {
        public override string Code => "CatalyticSurge";
        public override string IconPath => "destariamasteries:textures/masteries/alchemy/catalyticsurge.png";
        public override string Ability => "CatalyticSurge";
        public override int Column => 6;
        public override int MaxLevel => 5;
        public override int RequiredMasteryLevel => 30;
        public CatalyticSurgeSkill()
        {
            var parameters = new Dictionary<string, object> { { "Values", new float[] { 0.75f, 1.0f, 1.25f, 1.5f, 1.75f } } };
            Attributes.Add("PoisonHealConversion", new StatConfiguration("Array", parameters));
        }

        public override EnumSkillType SkillType => EnumSkillType.Active;

        public override string GetDisplayName(int level)
        {
            return Lang.Get("destariamasteries:alchemy-catalyticsurge-name");
        }

        public override string GetDescription(int level)
        {
            float healconversion = StatScalingUtil.GetScaledValue(GetValueSafe<StatConfiguration>("PoisonHealConversion"), level);
            return Lang.Get("destariamasteries:alchemy-catalyticsurge-desc", healconversion * 100);
        }
    }
}