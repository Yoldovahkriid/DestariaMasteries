using MasteryLibrary.src.Behaviors.EntityBehaviors;
using MasteryLibrary.src.Core.Effects;
using MasteryLibrary.src.Core.Masteries.Data;
using MasteryLibrary.src.Core.Masteries.Instances;
using MasteryLibrary.src.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace DestariaMasteries.src.Effects
{
    public class PoisonDOT : Effect
    {
        public override string Code => "PoisonDamageOverTime";

        public override string IconPath => string.Empty;

        public override bool CanStack => true;
        public override StackingBehavior StackingBehavior => StackingBehavior.ExtendDuration;

        private const string LAST_TICK_KEY = "LastTickTime";
        private const string TICK_INTERVAL_KEY = "TickInterval";
        private const string DAMAGE_PER_TICK_KEY = "DamagePerTick";

        public override void OnApply(EffectInstance instance, EntityAgent entity)
        {
            instance.CustomData.Add(LAST_TICK_KEY, 0L);
            instance.CustomData.Add(TICK_INTERVAL_KEY, 1000);
            if (entity is EntityPlayer entityPlayer)
            {
                entityPlayer.WatchedAttributes.SetBool("IsPoisoned", true);
                entityPlayer.WatchedAttributes.MarkPathDirty("IsPoisoned");
                EntityBehaviorPlayerMasteries? behavior = entity.GetBehavior<EntityBehaviorPlayerMasteries>();
                if (behavior != null) 
                {
                    SkillInstance? SInst = behavior.PlayerMasteryData.GetSkillInstance("ToxicReserves");
                    if (SInst == null) return;
                    StatConfiguration? walkspeed = SInst.Skill.GetValueSafe<StatConfiguration>("WalkSpeedBonus");
                    float hungerratereduction = SInst.Skill.GetValueSafe<float>("HungerRateReduction");

                    entity.Stats.Set("walkspeed", $"masterylib-effect-{SInst.Skill.Code}", StatScalingUtil.GetScaledValue(walkspeed, SInst.Level));
                    entity.Stats.Set("hungerrate", $"masterylib-effect-{SInst.Skill.Code}", hungerratereduction);
                }
            }
        }

        public override void OnTick(EffectInstance instance, EntityAgent entity, float dt)
        {
            long lastTick = instance.GetValueSafe<long>(LAST_TICK_KEY);
            int tickInterval = instance.GetValueSafe<int>(TICK_INTERVAL_KEY);
            long currentTime = entity.World.ElapsedMilliseconds;

            if (currentTime - lastTick >= tickInterval)
            {
                float damagePerTick = instance.GetValueSafe<float>(DAMAGE_PER_TICK_KEY);
                float totalDamage = damagePerTick * instance.Magnitude;
                DamageSource damageSource = new DamageSource()
                {
                    Source = EnumDamageSource.Internal,
                    Type = EnumDamageType.Poison
                };

                entity.ReceiveDamage(damageSource, totalDamage);
                instance.CustomData[LAST_TICK_KEY] = currentTime;

                // Poison particle effect
                entity.World.SpawnParticles(new SimpleParticleProperties()
                {
                    MinPos = entity.Pos.XYZ.AddCopy(-0.5, entity.LocalEyePos.Y / 2, -0.5),
                    AddPos = new Vec3d(1.0, 1.0, 1.0),

                    MinVelocity = new Vec3f(-0.2f, -0.1f, -0.2f),
                    AddVelocity = new Vec3f(0.4f, 0.5f, 0.4f),

                    MinSize = 0.5f,
                    MaxSize = 1.2f,

                    Color = ColorUtil.ToRgba(255, 50, 200, 50),
                    LifeLength = 1.5f,
                    MinQuantity = 15,
                    GravityEffect = 0.25f,
                    ParticleModel = EnumParticleModel.Cube
                });
            }
        }

        public override void OnRemove(EffectInstance instance, EntityAgent entity, RemovalReason reason)
        {
            if (entity is EntityPlayer entityPlayer)
            {
                entityPlayer.WatchedAttributes.SetBool("IsPoisoned", false);
                entityPlayer.WatchedAttributes.MarkPathDirty("IsPoisoned");
                EntityBehaviorPlayerMasteries? behavior = entity.GetBehavior<EntityBehaviorPlayerMasteries>();
                if (behavior != null)
                {
                    SkillInstance? SInst = behavior.PlayerMasteryData.GetSkillInstance("ToxicReserves");
                    if (SInst == null) return;

                    entity.Stats.Remove("walkspeed", $"masterylib-effect-{SInst.Skill.Code}");
                    entity.Stats.Remove("hungerrate", $"masterylib-effect-{SInst.Skill.Code}");
                }
            }
        }
    }
}
