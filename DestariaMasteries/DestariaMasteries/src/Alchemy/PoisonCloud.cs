using Cairo;
using MasteryLibrary;
using MasteryLibrary.src.Behaviors.EntityBehaviors;
using MasteryLibrary.src.Core.Abilities;
using MasteryLibrary.src.Core.Effects;
using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace DestariaMasteries.src.Alchemy
{
    public class PoisonCloud : Ability
    {
        private const long POISONDURATIONMS = 5000;
        private const int AOE_TICK_INTERVAL_MS = 250;
        public override string Code => "PoisonCloud";
        public override AbilityResult CanUse(AbilityContext context)
        {
            ItemStack? heldItem = context.Player.InventoryManager.ActiveHotbarSlot.Itemstack;
            if (heldItem == null) return AbilityResult.FailureResult();
            if (!heldItem.Collectible.Code.Path.Contains("mushroom")) return AbilityResult.FailureResult();
            return AbilityResult.SuccessResult();
        }
        public override AbilityResult Execute(AbilityContext context)
        {
            IServerPlayer player = context.Player;
            ICoreServerAPI api = context.API;
            CollectibleObject? collectible = player.InventoryManager.ActiveHotbarSlot.Itemstack?.Collectible;
            if (collectible == null) return AbilityResult.FailureResult();
            //Since CanUse shoudnt have failed we can assume the held item is a mushroom
            FoodNutritionProperties nutriprops = collectible.NutritionProps;
            float damagepertick = (nutriprops.Health / 10) * player.Entity.Stats.GetBlended("poisonDamageMul");

            context.Player.InventoryManager.ActiveHotbarSlot.TakeOut(1);
            context.Player.InventoryManager.ActiveHotbarSlot.MarkDirty();

            Vec3d castPos = player.Entity.Pos.XYZ.Clone();
            SpawnParticle(player, context.Level);

            float elapsedseconds = 0;
            long listenerid = 0;
            float totaltime = 5.0f + 2.0f * (context.Level - 1);

            listenerid = api.Event.RegisterGameTickListener((float dt) => { 
                elapsedseconds += dt;
                ApplyPoisonToEntitiesInRange(api, castPos, 3.5f, 3.5f, damagepertick, Code);
                if (elapsedseconds >= totaltime)
                {
                   api.Event.UnregisterGameTickListener(listenerid);
                }
            }, AOE_TICK_INTERVAL_MS);
            
            return AbilityResult.SuccessResult();
        }

        private void SpawnParticle(IServerPlayer player, int level)
        {

            float radius = 3.5f;
            float height = 4.0f;

            SimpleParticleProperties smoke = new SimpleParticleProperties(
                400, 600,
                ColorUtil.ColorFromRgba(50, 200, 50, 210),

                new Vec3d(),
                new Vec3d(radius * 2, height, radius * 2),

                new Vec3f(-0.3f, -0.1f, -0.3f), // MinVelocity
                new Vec3f(0.3f, 0.2f, 0.3f),    // MaxVelocity
                5.0f,                           // Base life length
                0.001f,                         // Gravity effect
                3.5f,                           // Min size
                7.5f,                           // Max size
                EnumParticleModel.Quad
            );

            smoke.MinPos.Set(player.Entity.Pos.X - radius, player.Entity.Pos.Y, player.Entity.Pos.Z - radius);

            smoke.SizeEvolve = EvolvingNatFloat.create(EnumTransformFunction.LINEAR, 2f);
            smoke.OpacityEvolve = EvolvingNatFloat.create(EnumTransformFunction.LINEAR, -255f);
            smoke.addLifeLength = 2.0f * (level - 1);

            player.Entity.World.SpawnParticles(smoke);
        }

        private void ApplyPoisonToEntitiesInRange(ICoreServerAPI api, Vec3d pos, float radius, float height, float damagepertick, string source)
        {
            Entity[] entities = api.World.GetEntitiesAround(pos, radius, height, e => e is EntityAgent);
            foreach (Entity entity in entities) { 
                if (entity is not EntityAgent agent) continue;
                EntityBehaviorEffects? behavior = agent.GetBehavior<EntityBehaviorEffects>();
                if (behavior == null) continue;
                MasteryLibraryAPI? masteryAPI = api.ModLoader.GetModSystem<MasteryLibraryAPI>();
                if (masteryAPI == null) continue;

                EffectInstance? instance = masteryAPI.EffectRegistry.CreateEffectInstance("PoisonDamageOverTime", source, POISONDURATIONMS, 1);
                if (instance == null) continue;
                instance.CustomData.Add("DamagePerTick", -damagepertick);
                api.Logger.Debug($"{instance.GetValueSafe<float>("DamagePerTick")}");
                behavior.EffectManager.AddEffect(instance);
            }
        }
    }
}
