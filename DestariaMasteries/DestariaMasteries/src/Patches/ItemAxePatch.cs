using HarmonyLib;
using MasteryLibrary.src.Behaviors.EntityBehaviors;
using MasteryLibrary.src.Core.Masteries.Instances;
using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace DestariaMasteries.src.Patches
{
    [HarmonyPatch(typeof(ItemAxe), nameof(ItemAxe.OnBlockBrokenWith))]
    public static class ItemAxePatch
    {
        static void Prefix(
            ItemAxe __instance,
            IWorldAccessor world,
            Entity byEntity,
            ItemSlot itemslot,
            BlockSelection blockSel,
            float dropQuantityMultiplier,
            out ItemStack __state)
        {
            __state = null;

            if (world.Side != EnumAppSide.Server) return;
            if (blockSel == null) return;
            PlayerMasteryData? data = byEntity.GetBehavior<EntityBehaviorPlayerMasteries>()?.PlayerMasteryData;
            if (data == null || !data.HasSkill("EmeraldBoughs"));

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
                if (block == null || block.BlockMaterial != EnumBlockMaterial.Leaves) continue;

                BlockDropItemStack[] drops = block.Drops;
                if (drops == null) continue;

                foreach (var drop in drops)
                {
                    ItemStack? resolved = drop.ResolvedItemstack;
                    if (resolved?.Collectible?.Code == null) continue;

                    if (resolved.Collectible.Code.Path.Contains("treeseed"))
                    {
                        __state = resolved.Clone();
                        __state.StackSize = 1;
                        return;
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
            ItemStack __state)
        {
            if (!__result || __state == null) return;
            if (world.Side != EnumAppSide.Server) return;

            ItemStack seedStack = __state.Clone();
            IPlayer? byPlayer = (byEntity as EntityPlayer)?.Player;

            bool given = byPlayer != null && byPlayer.InventoryManager.TryGiveItemstack(seedStack, true);

            if (!given)
            {
                world.SpawnItemEntity(seedStack, blockSel.Position.ToVec3d().Add(0.5, 0.5, 0.5));
            }
        }

    }
}
