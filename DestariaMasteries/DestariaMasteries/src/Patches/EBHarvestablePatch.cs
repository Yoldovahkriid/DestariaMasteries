using DestariaMasteries.src.Utils;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.GameContent;

namespace DestariaMasteries.src.Patches
{
    [HarmonyPatch(typeof(EntityBehaviorHarvestable), "get_dropQuantityMultiplier")]
    public class EBHarvestablePatch
    {
        public static void Postfix(EntityBehaviorHarvestable __instance, ref float __result)
        {
            if (__instance.entity.WatchedAttributes.HasAttribute("deathDamageType") && __instance.entity.WatchedAttributes.GetInt("deathDamageType") == ModConstants.CorpseMangledValue)
            {
                __result = 0.05f;
            }

            if (__instance.entity.WatchedAttributes.HasAttribute("ToTheBone") && __instance.entity.WatchedAttributes.GetBool("ToTheBone"))
            {
                __result *= 1.5f;
            }
        }
    }
}
