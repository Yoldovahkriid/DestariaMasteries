using MasteryLibrary.src.Behaviors.CollectibleBehaviors;
using MasteryLibrary.src.Core.Abilities;
using MasteryLibrary.src.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace DestariaMasteries.src.Alchemy
{
    public class PrimedToxin: Ability
    {
        public override string Code => "PrimedToxin";

        public override AbilityResult Execute(AbilityContext context)
        {
            if (context.Player == null) return AbilityResult.FailureResult("No Player");
            IServerPlayer player = context.Player;
            ItemStack? ActiveItem = player.InventoryManager.ActiveHotbarSlot.Itemstack;
            if (ActiveItem == null) return AbilityResult.FailureResult();
            if (!ActiveItem.Collectible.HasBehavior<CanInflictEffects>()) return AbilityResult.FailureResult();

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
            CanInflictEffects itemeffects = ActiveItem.Collectible.GetBehavior<CanInflictEffects>();
            EffectConfiguration effectConfig = new EffectConfiguration
            {
                EffectCode = "PoisonDamageOverTime",
                DurationMs = (int)duration * (1000),
                Magnitude = 1,
                CustomData = new Dictionary<string, object>()
            };
            effectConfig.CustomData.Add("DamagePerTick", damage);
            itemeffects.AddEffect(effectConfig);
            return AbilityResult.SuccessResult();
        }
    }
}
