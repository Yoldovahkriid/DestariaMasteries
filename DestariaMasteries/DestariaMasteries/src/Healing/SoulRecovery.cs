using MasteryLibrary.src.Behaviors.EntityBehaviors;
using MasteryLibrary.src.Core.Abilities;
using MasteryLibrary.src.Core.Masteries.Instances;
using MasteryLibrary.src.Utilities;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace DestariaMasteries.src.Healing
{
    public class SoulRecovery : Ability
    {
        private const float SoulTouchRadius = 8f;

        public override string Code => "SoulRecovery";

        public override AbilityResult CanUse(AbilityContext context)
        {
            EntityBehaviorHealth? health = context.Player.Entity.GetBehavior<EntityBehaviorHealth>();
            if (health == null) return AbilityResult.FailureResult("No health behavior found.");
            if (health.Health >= health.MaxHealth) return AbilityResult.FailureResult("Already at full health.");

            return AbilityResult.SuccessResult();
        }

        public override AbilityResult Execute(AbilityContext context)
        {
            EntityBehaviorHealth? health = context.Player.Entity.GetBehavior<EntityBehaviorHealth>();
            if (health == null) return AbilityResult.FailureResult("No health behavior found.");

            StatConfiguration? healConfig = context.GetValueSafe<StatConfiguration>("MissingHealthHealPercent");
            float healPercent = StatScalingUtil.GetScaledValue(healConfig, context.Level);
            float missingHealth = health.MaxHealth - health.Health;
            float healAmount = missingHealth * healPercent;

            if (healAmount <= 0) return AbilityResult.FailureResult("Already at full health.");

            float healthBeforeHeal = health.Health;

            DamageSource healSource = new DamageSource()
            {
                Source = EnumDamageSource.Internal,
                Type = EnumDamageType.Heal
            };

            context.Player.Entity.ReceiveDamage(healSource, healAmount);

            float actualHealAmount = health.Health - healthBeforeHeal;
            ApplySoulTouch(context, actualHealAmount, healSource);

            SpawnSoulRecoveryParticles(context.Player.Entity);

            return AbilityResult.SuccessResult();
        }

        private void ApplySoulTouch(AbilityContext context, float actualHealAmount, DamageSource healSource)
        {
            if (actualHealAmount <= 0) return;

            SkillInstance? soulTouch = context.Player.Entity
                .GetBehavior<EntityBehaviorPlayerMasteries>()
                ?.PlayerMasteryData.GetSkillInstance("SoulTouch");

            if (soulTouch == null) return;

            StatConfiguration? sharedHealConfig = soulTouch.Skill.GetValueSafe<StatConfiguration>("SharedHealPercent");
            float sharedHealPercent = StatScalingUtil.GetScaledValue(sharedHealConfig, soulTouch.Level);
            float sharedHealAmount = actualHealAmount * sharedHealPercent;
            if (sharedHealAmount <= 0) return;

            Entity[] nearbyEntities = context.API.World.GetEntitiesAround(
                context.Player.Entity.Pos.XYZ,
                SoulTouchRadius,
                SoulTouchRadius,
                entity => entity is EntityPlayer && entity.EntityId != context.Player.Entity.EntityId
            );

            foreach (Entity entity in nearbyEntities)
            {
                if (entity is not EntityPlayer nearbyPlayer) continue;
                if (nearbyPlayer.PlayerUID == context.Player.PlayerUID) continue;

                nearbyPlayer.ReceiveDamage(healSource, sharedHealAmount);
                SpawnSoulTouchParticles(nearbyPlayer);
            }
        }

        private void SpawnSoulRecoveryParticles(EntityAgent entity)
        {
            SimpleParticleProperties particles = new SimpleParticleProperties()
            {
                MinPos = entity.Pos.XYZ.AddCopy(-0.85, 0.1, -0.85),
                AddPos = new Vec3d(1.7, 1.9, 1.7),

                MinVelocity = new Vec3f(-0.9f, 0.15f, -0.9f),
                AddVelocity = new Vec3f(1.8f, 1.3f, 1.8f),

                MinSize = 0.35f,
                MaxSize = 1.1f,

                Color = ColorUtil.ToRgba(220, 255, 214, 60),
                LifeLength = 1.2f,
                MinQuantity = 80,
                AddQuantity = 30,
                GravityEffect = -0.02f,
                ParticleModel = EnumParticleModel.Quad
            };

            particles.SizeEvolve = EvolvingNatFloat.create(EnumTransformFunction.LINEAR, -0.5f);
            particles.OpacityEvolve = EvolvingNatFloat.create(EnumTransformFunction.LINEAR, -220f);

            entity.World.SpawnParticles(particles);
        }

        private void SpawnSoulTouchParticles(EntityAgent entity)
        {
            SimpleParticleProperties particles = new SimpleParticleProperties()
            {
                MinPos = entity.Pos.XYZ.AddCopy(-0.55, 0.2, -0.55),
                AddPos = new Vec3d(1.1, 1.4, 1.1),

                MinVelocity = new Vec3f(-0.25f, 0.05f, -0.25f),
                AddVelocity = new Vec3f(0.5f, 0.7f, 0.5f),

                MinSize = 0.25f,
                MaxSize = 0.75f,

                Color = ColorUtil.ToRgba(190, 255, 224, 90),
                LifeLength = 0.9f,
                MinQuantity = 25,
                AddQuantity = 15,
                GravityEffect = -0.01f,
                ParticleModel = EnumParticleModel.Quad
            };

            particles.SizeEvolve = EvolvingNatFloat.create(EnumTransformFunction.LINEAR, -0.35f);
            particles.OpacityEvolve = EvolvingNatFloat.create(EnumTransformFunction.LINEAR, -180f);

            entity.World.SpawnParticles(particles);
        }
    }
}
