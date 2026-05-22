using MasteryLibrary;
using MasteryLibrary.src.Behaviors.CollectibleBehaviors;
using MasteryLibrary.src.Behaviors.EntityBehaviors;
using MasteryLibrary.src.Core.Abilities;
using MasteryLibrary.src.Core.Effects;
using MasteryLibrary.src.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace DestariaMasteries.src.Alchemy
{
    public class CostlyMistake : Ability
    {
        public override string Code => "CostlyMistake";

        public override AbilityResult Execute(AbilityContext context)
        {
            if (context.Player == null) return AbilityResult.FailureResult("No Player");
            IServerPlayer player = context.Player;
            EntityBehaviorEffects? effectBehavior = player.Entity.GetBehavior<EntityBehaviorEffects>();
            if (effectBehavior == null) return AbilityResult.FailureResult("Player is missing the effect behavior");

            float duration = 0;
            float damage = 1.0f;
            if (context.CustomData != null && context.GetValueSafe<StatConfiguration>("PoisonDuration") != null)
            {
                StatConfiguration? statConfig = context.GetValueSafe<StatConfiguration>("PoisonDuration");
                if (statConfig != null)
                {
                    duration = StatScalingUtil.GetScaledValue(statConfig, context.Level);
                }

                damage = context.GetValueSafe<float>("PoisonDamage");
            }
            if (duration <= 0) return AbilityResult.FailureResult("Failed to resolve poison duration.");
            damage *= context.Player.Entity.Stats.GetBlended("poisonDamageMul");
            EffectInstance? effect = context.API.ModLoader.GetModSystem<MasteryLibraryAPI>()?.EffectRegistry.CreateEffectInstance("PoisonDamageOverTime", Code, (long)duration * 1000, 1);
            if (effect == null) return AbilityResult.FailureResult("Failed to create effect");
            effect.CustomData.Add("DamagePerTick", damage);
            effectBehavior.EffectManager.AddEffect(effect);
            return AbilityResult.SuccessResult();
        }
    }
}
