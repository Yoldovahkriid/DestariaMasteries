using HarmonyLib;
using MasteryLibrary.src.Behaviors.EntityBehaviors;
using MasteryLibrary.src.Core.Masteries.Instances;
using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace DestariaMasteries.src.Patches
{
    [HarmonyPatch(typeof(BlockPan), nameof(BlockPan.OnHeldInteractStop))]
    public static class BlockPanPatch
    {
        static void Prefix(BlockPan __instance, float secondsUsed, ItemSlot slot, out bool __state)
        => __state = secondsUsed >= 3.4f && __instance.GetBlockMaterialCode(slot.Itemstack) != null;

        static void Postfix(bool __state, EntityAgent byEntity)
        {
            if (!__state) return;
            PlayerMasteryData? data = byEntity?.GetBehavior<EntityBehaviorPlayerMasteries>()?.PlayerMasteryData;
            if (data != null && data.HasSkill("PanHandling"))
            {
                byEntity?.GetBehavior<EntityBehaviorHunger>()?.ConsumeSaturation(4f * byEntity.Stats.GetBlended("panSaturationConsumptionMul"));
            }
        }
    }
}
