using MasteryLibrary.src.Behaviors.EntityBehaviors;
using MasteryLibrary.src.Core.Abilities;
using MasteryLibrary.src.Core.Effects;
using MasteryLibrary.src.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Common;

namespace DestariaMasteries.src.Alchemy
{
    public class CatalyticSurge : Ability
    {
        public override string Code => "CatalyticSurge";
        public override AbilityResult CanUse(AbilityContext context)
        {
            EntityBehaviorEffects? behaviorEffects = context.Player.Entity.GetBehavior<EntityBehaviorEffects>();
            List<EffectInstance>? effectInstances = behaviorEffects?.EffectManager.GetActiveEffects();
            if (effectInstances == null || effectInstances.Count == 0) return AbilityResult.FailureResult();
            foreach (EffectInstance effectInstance in effectInstances) { 
                if(effectInstance.Effect.Code == "PoisonDamageOverTime")
                {
                    return AbilityResult.SuccessResult();
                }
            }
            return AbilityResult.FailureResult();
        }

        public override AbilityResult Execute(AbilityContext context)
        {
            EntityBehaviorEffects? behaviorEffects = context.Player.Entity.GetBehavior<EntityBehaviorEffects>();
            List<EffectInstance>? effectInstances = behaviorEffects?.EffectManager.GetActiveEffects();

            StatConfiguration? poisonconversion = context.GetValueSafe<StatConfiguration>("PoisonHealConversion");
            float healPercentage = poisonconversion == null ? 0.5f : StatScalingUtil.GetScaledValue(poisonconversion, context.Level);
            float totalRemainingDamage = 0;

            foreach (EffectInstance instance in effectInstances)
            {
                if (instance.Effect.Code == "PoisonDamageOverTime")
                {
                    int tickInterval = instance.GetValueSafe<int>("TickInterval");
                    if (tickInterval <= 0) tickInterval = 1000;

                    float damagePerTick = instance.GetValueSafe<float>("DamagePerTick");

                    long remainingTicks = instance.Duration / tickInterval;

                    totalRemainingDamage += remainingTicks * damagePerTick * instance.Magnitude;
                }
            }

            if (totalRemainingDamage > 0)
            {
                float healAmount = totalRemainingDamage * healPercentage;

                DamageSource healSource = new DamageSource()
                {
                    Source = EnumDamageSource.Internal,
                    Type = EnumDamageType.Heal
                };

                context.Player.Entity.ReceiveDamage(healSource, healAmount);
            }
            behaviorEffects?.EffectManager.RemoveEffect("PoisonDamageOverTime", RemovalReason.Dispelled);
            return AbilityResult.SuccessResult();
        }
    }
}
