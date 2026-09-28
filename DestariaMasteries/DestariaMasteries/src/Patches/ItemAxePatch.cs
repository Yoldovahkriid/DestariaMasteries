using DestariaMasteries.src.Utils;
using HarmonyLib;
using MasteryLibrary.src.Behaviors.EntityBehaviors;
using MasteryLibrary.src.Core.Masteries.Instances;
using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace DestariaMasteries.src.Patches
{
    [HarmonyPatch(typeof(ItemAxe), nameof(ItemAxe.OnBlockBrokenWith))]
    public static class ItemAxePatch
    {
        private class TreeBreakState
        {
            public ItemStack SeedStack;
            public float TotalXp;
        }

        static void Prefix(
            ItemAxe __instance,
            IWorldAccessor world,
            Entity byEntity,
            ItemSlot itemslot,
            BlockSelection blockSel,
            float dropQuantityMultiplier,
            out TreeBreakState __state)
        {
            __state = new TreeBreakState();

            if (world.Side != EnumAppSide.Server) return;
            if (blockSel == null) return;

            PlayerMasteryData? data = byEntity.GetBehavior<EntityBehaviorPlayerMasteries>()?.PlayerMasteryData;
            bool hasEmeraldBoughs = data != null && data.HasSkill("EmeraldBoughs");

            Stack<BlockPos> positions;
            try
            {
                positions = __instance.FindTree(world, blockSel.Position, out _, out _);
            }
            catch
            {
                return;
            }

            foreach (BlockPos pos in positions)
            {
                Block block = world.BlockAccessor.GetBlock(pos);
                if (block == null) continue;

                // Sum XP for every block in the tree rather than awarding per-block;
                // the total is granted once in the Postfix via OnTreeHarvest.
                __state.TotalXp += XpRewardEvaluator.GetXpForTreeBlock(block);

                if (!hasEmeraldBoughs || __state.SeedStack != null) continue;
                if (block.BlockMaterial != EnumBlockMaterial.Leaves) continue;

                BlockDropItemStack[] drops = block.Drops;
                if (drops == null) continue;

                foreach (var drop in drops)
                {
                    ItemStack? resolved = drop.ResolvedItemstack;
                    if (resolved?.Collectible?.Code == null) continue;

                    if (resolved.Collectible.Code.Path.Contains("treeseed"))
                    {
                        __state.SeedStack = resolved.Clone();
                        __state.SeedStack.StackSize = 1;
                        break;
                    }
                }
            }
        }

        static void Postfix(
            bool __result,
            ItemAxe __instance,
            IWorldAccessor world,
            Entity byEntity,
            ItemSlot itemslot,
            BlockSelection blockSel,
            float dropQuantityMultiplier,
            TreeBreakState __state)
        {
            if (!__result || __state == null) return;
            if (world.Side != EnumAppSide.Server) return;

            IPlayer? byPlayer = (byEntity as EntityPlayer)?.Player;

            if (__state.SeedStack != null)
            {
                ItemStack seedStack = __state.SeedStack.Clone();
                bool given = byPlayer != null && byPlayer.InventoryManager.TryGiveItemstack(seedStack, true);

                if (!given)
                {
                    world.SpawnItemEntity(seedStack, blockSel.Position.ToVec3d().Add(0.5, 0.5, 0.5));
                }
            }

            if (__state.TotalXp > 0f && byPlayer is IServerPlayer serverPlayer)
            {
                XpRewardEvaluator.OnTreeHarvest(serverPlayer, __state.TotalXp);
            }
        }

    }
}