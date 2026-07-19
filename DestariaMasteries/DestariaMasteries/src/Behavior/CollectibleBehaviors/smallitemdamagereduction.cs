using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;

namespace DestariaMasteries.src.Behavior.CollectibleBehaviors
{
    public class SmallItemDamageReduction : CollectibleBehavior
    {
        public SmallItemDamageReduction(CollectibleObject collObj) : base(collObj)
        {
        }

        public override void OnDamageItem(IWorldAccessor world, Entity byEntity, ItemSlot itemslot, ref int amount, ref EnumHandling bhHandling)
        {
            float damageReduction = byEntity.Stats.GetBlended("smallToolDamageReduction") -1f;
            damageReduction = Math.Clamp(damageReduction, 0f, 1f);

            if (amount <= 1)
            {
                float chanceToTakeDamage = 1f - damageReduction;
        
                if (world.Rand.NextDouble() >= chanceToTakeDamage)
                {
                    amount = 0;
                }
            }
            else
            {
                float exactDamage = amount * (1f - damageReduction);
                int baseDamage = (int)exactDamage;
                float remainder = exactDamage - baseDamage;

                if (world.Rand.NextDouble() < remainder)
                {
                    baseDamage++;
                }

                amount = baseDamage;
            }
        }
    }
}
