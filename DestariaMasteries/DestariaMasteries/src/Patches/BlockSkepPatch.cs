using HarmonyLib;
using MasteryLibrary.src.Behaviors.EntityBehaviors;
using MasteryLibrary.src.Core.Masteries.Instances;
using System;
using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace DestariaMasteries.src.Patches
{
    [HarmonyPatch(typeof(BlockSkep), nameof(BlockSkep.GetDrops))]
    public class BlockSkepPatch
    {
        static void Postfix(BlockSkep __instance, ref ItemStack[] __result, IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
        {
            if (world.Side == EnumAppSide.Server && __result != null && __result.Length > 0 && __instance.GetBlockEntity<BlockEntityBeehive>(pos)?.Harvestable == true)
            {
                PlayerMasteryData? data = byPlayer?.Entity?.GetBehavior<EntityBehaviorPlayerMasteries>()?.PlayerMasteryData;
                if (data == null || !data.HasSkill("AmberCombs")) return; // AmberCombs has a MaxLevel of 1 so we don't need to check the level, just if they have it.

                AssetLocation emptySkepCode = __instance.Code.CopyWithPath(__instance.Code.Path.Replace("populated", "empty"));
                AssetLocation finalCode = emptySkepCode.CopyWithPath(emptySkepCode.Path.Replace(__instance.Variant["side"] ?? "east", "east"));

                Block emptySkepBlock = world.GetBlock(finalCode);

                List<ItemStack> newDrops = new List<ItemStack>
                {
                    new ItemStack(emptySkepBlock)
                };

                foreach (ItemStack stack in __result)
                {
                    if (stack.Collectible.Code.Path.Contains("honeycomb"))
                    {
                        stack.StackSize = (int)Math.Round((stack.StackSize + 0.5) * 1.5f); // +0.5 to round up so the bonus can be up to 2 more combs
                        newDrops.Add(stack);
                    }
                }

                __result = newDrops.ToArray();
            }
        }
    }
}