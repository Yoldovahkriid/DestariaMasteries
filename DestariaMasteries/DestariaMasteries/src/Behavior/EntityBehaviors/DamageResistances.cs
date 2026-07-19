using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using static OpenTK.Graphics.OpenGL.GL;

namespace DestariaMasteries.src.Behavior.EntityBehaviors
{
    public class DamageResistances : EntityBehavior
    {
        EntityPlayer? entityPlayer { get; set; }
        TagSetFast RustMonsters;
        public DamageResistances(Entity entity) : base(entity)
        {
            this.entityPlayer = entity as EntityPlayer;
        }

        public override void Initialize(EntityProperties properties, JsonObject attributes)
        {
            base.Initialize(properties, attributes);

            entity.Api.EntityTagRegistry.TryCreateTagSetAndLogIssues(out RustMonsters, "rust-creature");
        }

        public override string PropertyName()
        {
            return "DamageResistances";
        }

        public override void OnEntityReceiveDamage(DamageSource damageSource, ref float damage)
        {
            Entity entity = damageSource.GetCauseEntity();
            if (entityPlayer == null || entity == null ||damageSource.Type == EnumDamageType.Heal)
            {
                base.OnEntityReceiveDamage(damageSource, ref damage);
                return;
            }

            float totalmultiplier = 1f;

            float damageResistanceFromMechanicals = entityPlayer.Stats.GetBlended("damageResistanceAgainstMechanicals");
            bool isMechanical = entity.Properties.Attributes?["isMechanical"].AsBool() ?? false;
            if (damageResistanceFromMechanicals > 0.0f && isMechanical)
            {
                totalmultiplier *= damageResistanceFromMechanicals;
            }

            float damageResistanceAgainstRust = entityPlayer.Stats.GetBlended("damageResistanceAgainstRust");
            if (damageResistanceAgainstRust > 0 && entity.Tags.Overlaps(RustMonsters))
            {
                entity.Api.Logger.Debug($"Rust multiplier Multiplier: {damageResistanceAgainstRust}");
                totalmultiplier *= damageResistanceAgainstRust;
            }

            entity.Api.Logger.Debug($"Total Multiplier: {totalmultiplier}");
            damage *= totalmultiplier;
        }
    }
}
