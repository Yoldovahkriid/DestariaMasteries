using DestariaMasteries.src.Behavior.EntityBehaviors;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace DestariaMasteries.src.Patches
{
    [HarmonyPatch(typeof(EntityBehaviorHealth), "OnEntityReceiveDamage")]
    public class EBHealthPatch
    {
        public static bool Prefix(EntityBehaviorHealth __instance, DamageSource damageSource, ref float damage)
        {
            __instance.entity.GetBehavior<DamageResistances>()?.OnEntityReceiveDamage(damageSource, ref damage);
            return true;
        }
    }
}
