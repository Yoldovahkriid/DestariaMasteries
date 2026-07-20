using DestariaMasteries.src.Utils;
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
        private bool tagsInitialized = false;

        public override string Code => "TemporalDevastation";
        TagSetFast creatureTags;

        public override AbilityResult Execute(AbilityContext context)
        {
            IPlayerInventoryManager inventoryManager = context.Player.InventoryManager;
            ItemSlot activeSlot = inventoryManager.ActiveHotbarSlot;
            ItemStack? itemStack = activeSlot.Itemstack;

            if (itemStack == null) return AbilityResult.FailureResult("No item in active hotbar slot.");

            if (itemStack.Collectible?.Code?.Path != "gear-temporal")
                return AbilityResult.FailureResult("Item in active hotbar slot is not a temporal gear.");

            if (!tagsInitialized)
            {
                context.API.EntityTagRegistry.TryCreateTagSet(out creatureTags, "rust-creature");
                tagsInitialized = true;
            }

            StatConfiguration? damageconfig = context.GetValueSafe<StatConfiguration>("SkillDamage");
            StatConfiguration? stabilityLossConfig = context.GetValueSafe<StatConfiguration>("LostStability");
            StatConfiguration? speedLossConfig = context.GetValueSafe<StatConfiguration>("SlowAmount");
            int skillRange = context.GetValueSafe<int>("SkillRange");

            float damage = 10f;
            if (damageconfig != null) damage = StatScalingUtil.GetScaledValue(damageconfig, context.Level);

            float stabilityLoss = 0.1f;
            if (stabilityLossConfig != null) stabilityLoss = StatScalingUtil.GetScaledValue(stabilityLossConfig, context.Level);

            ApplyDamageAndEffects(context.API, context.Player.Entity.Pos.XYZ.Clone(), skillRange, damage, stabilityLoss, speedLossConfig, context.Level, context.Player);

            activeSlot.TakeOut(1);
            activeSlot.MarkDirty();

            return AbilityResult.SuccessResult();
        }

        public void ApplyDamageAndEffects(ICoreServerAPI api, Vec3d pos, float radius, float damage, float stabilityLoss, StatConfiguration speedLoss, int level, IServerPlayer player)
        {
            Entity[] entities = api.World.GetEntitiesAround(pos, radius, radius, e => e is EntityAgent);
            foreach (Entity entity in entities)
            {
                if (entity is not EntityAgent agent) continue;

                if (agent is not EntityPlayer && agent.Tags.Overlaps(creatureTags))
                {
                    agent.ReceiveDamage(new DamageSource { SourceEntity = player.Entity, Source = EnumDamageSource.Explosion, Type = EnumDamageType.Poison, IgnoreInvFrames = true }, damage);
                    if (!entity.Alive)
                    {
                        entity.WatchedAttributes.SetInt("deathDamageType", ModConstants.CorpseMangledValue);
                    }
                    continue;
                }

                EntityBehaviorEffects? behavior = agent.GetBehavior<EntityBehaviorEffects>();
                if (behavior == null) continue;

                MasteryLibraryAPI? masteryAPI = api.ModLoader.GetModSystem<MasteryLibraryAPI>();
                if (masteryAPI == null) continue;

                EffectInstance? drainEffect = masteryAPI.EffectRegistry.CreateEffectInstance("StabilityDrain", this.Code, DRAINDURATIONMS, 1);
                if (drainEffect == null) continue;

                drainEffect.CustomData.Add("DrainPerTick", stabilityLoss / 4f); // Effects tick roughly every 250ms so to make the effect not instant we devide by 4 to spread the effect
                behavior.EffectManager.AddEffect(drainEffect);

                EffectInstance? speedLossEffect = masteryAPI.EffectRegistry.CreateEffectInstance("StatBoost", this.Code, SPEEDLOSSDURATIONMS, level);
                if (speedLossEffect == null) continue;

                speedLossEffect.CustomData.Add("ActiveStats", new Dictionary<string, StatConfiguration> { { "walkspeed", speedLoss } });
                behavior.EffectManager.AddEffect(speedLossEffect);
            }
        }
    }
}
