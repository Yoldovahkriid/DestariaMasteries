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
            float damageReduction = byEntity.Stats.GetBlended("smallToolDamageReduction") - 1f;
            damageReduction = Math.Clamp(damageReduction, 0f, 1f);

            float chanceToTakeDamage = 1f - damageReduction;
            int finalDamage = 0;

            for (int i = 0; i < amount; i++)
            {
                if (world.Rand.NextDouble() < chanceToTakeDamage)
                {
                    finalDamage++;
                }
            }

            amount = finalDamage;
        }
    }
}
