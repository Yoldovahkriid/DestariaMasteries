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
    [HarmonyPatch(typeof(BEBehaviorFruitingBush), nameof(BEBehaviorFruitingBush.SetHarvested))]
    public static class BEBehaviorFruitingBushPatch
    {
        static void Postfix(BEBehaviorFruitingBush __instance, IPlayer byPlayer, float dropQuantityMultiplier = 1)
        {
            PlayerMasteryData? data = byPlayer?.Entity?.GetBehavior<EntityBehaviorPlayerMasteries>()?.PlayerMasteryData;
            if (data == null || !data.HasSkill("EmeraldBoughs")) return;

            var api = __instance.Api;
            var block = api.World.GetBlock(AssetLocation.Create(__instance.Block.Attributes["cuttingBlockCode"].AsString(), __instance.Block.Code.Domain));

            var cuttingStack = new ItemStack(block);
            cuttingStack.Attributes.SetString("traits", string.Join(",", __instance.BState.Traits));

            if (!byPlayer.InventoryManager.TryGiveItemstack(cuttingStack))
            {
                api.World.SpawnItemEntity(cuttingStack, __instance.Pos.ToVec3d().Add(0.5, 0.5, 0.5));
            }
        }
    }
}
