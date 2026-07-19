using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;

namespace DestariaMasteries.src.Behavior.CollectibleBehaviors
{
    public class DamageScalarBehavior : CollectibleBehavior
    {
        public EntityPlayer? Player = null;
        public DamageScalarBehavior(CollectibleObject collObj) : base(collObj) { }
        public override float GetDamageToEntity(float baseDamage, Entity entity, ItemStack itemStack, ref bool isCriticalHit, ref EnumHandling handling)
        {
            if (Player != null)
            {
                if (entity.Properties.Attributes?["isMechanical"].AsBool() == true)
                {
                    handling = EnumHandling.Handled;
                    float multiplier = Player.Stats.GetBlended("TuningSpearDamageAgainstMechanicals");
                    return baseDamage * multiplier;
                }
            }
            return base.GetDamageToEntity(baseDamage, entity, itemStack, ref isCriticalHit, ref handling);
        }
    }
}
