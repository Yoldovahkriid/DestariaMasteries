using MasteryLibrary;
using MasteryLibrary.src.Behaviors.EntityBehaviors;
using MasteryLibrary.src.Core.Abilities;
using MasteryLibrary.src.Core.Effects;
using MasteryLibrary.src.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace DestariaMasteries.src.Mechanist
{
    public class TemporalDevastation : Ability
    {
        private const long DRAINDURATIONMS = 1000L;
        private const long SPEEDLOSSDURATIONMS = 2000L;

        public override string Code => "TemporalDevastation";
        TagSetFast creatureTags;
        public override AbilityResult Execute(AbilityContext context)
        {
            IPlayerInventoryManager inventoryManager = context.Player.InventoryManager;
            ItemSlot activeSlot = inventoryManager.ActiveHotbarSlot;
            ItemStack? itemStack = activeSlot.Itemstack;

            if (itemStack == null) return AbilityResult.FailureResult("No item in active hotbar slot.");
            if (itemStack.Collectible.Code.Path != "gear-temporal") return AbilityResult.FailureResult("Item in active hotbar slot is not a temporal gear.");

            StatConfiguration? damageconfig = context.GetValueSafe<StatConfiguration>("SkillDamage");
            StatConfiguration? stabilityLossConfig = context.GetValueSafe<StatConfiguration>("LostStability");
            StatConfiguration? speedLossConfig = context.GetValueSafe<StatConfiguration>("SlowAmount");
            int skillRange = context.GetValueSafe<int>("SkillRange");

            float damage = 10f;
            if (damageconfig != null) damage = StatScalingUtil.GetScaledValue(damageconfig, context.Level);
            float stabilityLoss = 0.1f;
            if (stabilityLossConfig != null) stabilityLoss = StatScalingUtil.GetScaledValue(stabilityLossConfig, context.Level);

            context.API.EntityTagRegistry.TryCreateTagSet(out creatureTags, "rust-creature");

            ApplyDamageAndEffects(context.API, context.Player.Entity.Pos.XYZ.Clone(), skillRange, damage, stabilityLoss, speedLossConfig, context.Level);
            activeSlot.TakeOut(1);
            activeSlot.MarkDirty();

            return AbilityResult.SuccessResult();
        }

        public void ApplyDamageAndEffects(ICoreServerAPI api, Vec3d pos, float radius, float damage, float stabilityLoss, StatConfiguration speedLoss, int level)
        {
            Entity[] entities = api.World.GetEntitiesAround(pos, radius, radius, e => e is EntityAgent);
            foreach (Entity entity in entities)
            {
                if (entity is not EntityAgent agent) continue;
                if (agent is not EntityPlayer && agent.Tags.Overlaps(creatureTags))
                {
                    agent.ReceiveDamage(new DamageSource { Source = EnumDamageSource.Unknown, Type = EnumDamageType.Poison, IgnoreInvFrames = true }, damage);
                    continue;
                }

                EntityBehaviorEffects? behavior = agent.GetBehavior<EntityBehaviorEffects>();
                if (behavior == null) continue;
                MasteryLibraryAPI? masteryAPI = api.ModLoader.GetModSystem<MasteryLibraryAPI>();
                if (masteryAPI == null) continue;

                EffectInstance? drainEffect = masteryAPI.EffectRegistry.CreateEffectInstance("StabilityDrain", this.Code, DRAINDURATIONMS, 1);
                if (drainEffect == null) continue;
                drainEffect.CustomData.Add("DrainPerTick", stabilityLoss / 4); //We devide by 4 so we are only applying 1/4 of the effect per tick (the effect ticks roughly every 250ms)
                behavior.EffectManager.AddEffect(drainEffect);

                EffectInstance? speedLossEffect = masteryAPI.EffectRegistry.CreateEffectInstance("StatBoost", this.Code, SPEEDLOSSDURATIONMS, level);
                if (speedLossEffect == null) continue;
                speedLossEffect.CustomData.Add("ActiveStats", new Dictionary<string, StatConfiguration> { { "walkspeed", speedLoss } });
                behavior.EffectManager.AddEffect(speedLossEffect);
            }
        }
    }
}
