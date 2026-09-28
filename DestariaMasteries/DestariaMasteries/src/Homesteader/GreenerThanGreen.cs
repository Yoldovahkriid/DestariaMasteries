using MasteryLibrary.src.Core.Abilities;
using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace DestariaMasteries.src.Homesteader
{
    public class GreenerThanGreen : Ability
    {
        private const int RequiredFertilizerPortions = 9;

        public override string Code => "GreenerThanGreen";

        public override AbilityResult CanUse(AbilityContext context)
        {
            IServerPlayer player = context.Player;

            BlockPos centerPos = GetTargetPos(context);
            if (centerPos == null)
            {
                return AbilityResult.FailureResult("You must be looking at a crop to use this ability.");
            }

            if (!AreaContainsCrop(context, centerPos))
            {
                return AbilityResult.FailureResult("There are no crops in the targeted 3x3 area.");
            }

            int portions = CountFertilizerPortions(player);
            if (portions < RequiredFertilizerPortions)
            {
                return AbilityResult.FailureResult(
                    $"You need {RequiredFertilizerPortions} portions of fertilizer to use this ability (you have {portions})."
                );
            }

            return AbilityResult.SuccessResult();
        }

        public override AbilityResult Execute(AbilityContext context)
        {
            IServerPlayer player = context.Player;
            ICoreServerAPI api = context.API;
            IBlockAccessor blockAccessor = api.World.BlockAccessor;

            BlockPos centerPos = GetTargetPos(context);
            if (centerPos == null)
            {
                return AbilityResult.FailureResult("You must be looking at a crop to use this ability.");
            }

            // Consume the fertilizer up front; if we somehow can't, bail out.
            if (!ConsumeFertilizerPortions(player, RequiredFertilizerPortions))
            {
                return AbilityResult.FailureResult(
                    $"You need {RequiredFertilizerPortions} portions of fertilizer to use this ability."
                );
            }

            int cropsGrown = 0;

            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dz = -1; dz <= 1; dz++)
                {
                    BlockPos pos = centerPos.AddCopy(dx, 0, dz);

                    if (GrowCropToMaturity(api, pos))
                    {
                        cropsGrown++;
                    }
                }
            }

            if (cropsGrown == 0)
            {
                return AbilityResult.FailureResult("No crops were left to grow in the target area.");
            }

            return AbilityResult.SuccessResult(
                $"You have used the Greener Than Green ability! {cropsGrown} crop(s) accelerated to full maturity.",
                cropsGrown
            );
        }

        private BlockPos GetTargetPos(AbilityContext context)
        {
            BlockPos? customPos = context.GetValueSafe<BlockPos>("targetPos");
            if (customPos != null)
            {
                return customPos;
            }

            return context.Player?.CurrentBlockSelection?.Position;
        }

        private bool AreaContainsCrop(AbilityContext context, BlockPos centerPos)
        {
            IBlockAccessor blockAccessor = context.API.World.BlockAccessor;

            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dz = -1; dz <= 1; dz++)
                {
                    BlockPos pos = centerPos.AddCopy(dx, 0, dz);
                    if (blockAccessor.GetBlock(pos) is BlockCrop)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool GrowCropToMaturity(ICoreServerAPI api, BlockPos pos)
        {
            IBlockAccessor blockAccessor = api.World.BlockAccessor;

            if (!(blockAccessor.GetBlock(pos) is BlockCrop cropBlock))
            {
                return false;
            }

            if (cropBlock.CropProps == null)
            {
                return false;
            }

            int maxStage = cropBlock.CropProps.GrowthStages;
            string currentStageStr = cropBlock.LastCodePart();

            if (!int.TryParse(currentStageStr, out int currentStage))
            {
                return false;
            }

            if (currentStage >= maxStage)
            {
                return false;
            }

            Block finalStageBlock = api.World.GetBlock(cropBlock.CodeWithParts(maxStage.ToString()));
            if (finalStageBlock == null)
            {
                return false;
            }

            blockAccessor.SetBlock(finalStageBlock.BlockId, pos);
            blockAccessor.TriggerNeighbourBlockUpdate(pos);
            blockAccessor.MarkBlockDirty(pos);

            BlockEntity be = blockAccessor.GetBlockEntity(pos.DownCopy());
            if (be is BlockEntityFarmland farmland)
            {
                farmland.MarkDirty(true);
            }

            return true;
        }

        private int CountFertilizerPortions(IServerPlayer player)
        {
            int total = 0;

            if (player?.InventoryManager?.Inventories == null)
            {
                return 0;
            }

            foreach (var inventory in player.InventoryManager.Inventories.Values)
            {
                foreach (var slot in inventory)
                {
                    if (slot?.Itemstack == null)
                    {
                        continue;
                    }

                    if (IsFertilizer(slot.Itemstack))
                    {
                        total += slot.Itemstack.StackSize;
                    }
                }
            }

            return total;
        }

        private bool ConsumeFertilizerPortions(IServerPlayer player, int amount)
        {
            if (CountFertilizerPortions(player) < amount)
            {
                return false;
            }

            int remaining = amount;

            foreach (var inventory in player.InventoryManager.Inventories.Values)
            {
                if (remaining <= 0)
                {
                    break;
                }

                foreach (var slot in inventory)
                {
                    if (remaining <= 0)
                    {
                        break;
                    }

                    if (slot?.Itemstack == null || !IsFertilizer(slot.Itemstack))
                    {
                        continue;
                    }

                    int take = Math.Min(remaining, slot.Itemstack.StackSize);
                    slot.TakeOut(take);
                    slot.MarkDirty();
                    remaining -= take;
                }
            }

            return remaining <= 0;
        }

        private bool IsFertilizer(ItemStack stack)
        {
            string codePath = stack.Collectible?.Code?.Path ?? "";
            return codePath.Contains("bonemeal")
                || codePath.Contains("potash")
                || codePath.Contains("saltpeter")
                || codePath.Contains("compost");
        }
    }
}