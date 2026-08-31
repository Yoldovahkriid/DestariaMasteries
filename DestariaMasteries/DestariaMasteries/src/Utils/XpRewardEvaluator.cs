using MasteryLibrary.src.Behaviors.EntityBehaviors;
using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.API.Util;

namespace DestariaMasteries.src.Utils
{
    public enum BlockTypeEnum
    {
        Ore,
        Crop,
        NoXP
    }

    public static class XpRewardEvaluator
    {
        private static AssetLocation oreWildcard = new AssetLocation("*:ore-*-*-*");
        private static AssetLocation cropWildcard = new AssetLocation("*:crop-*-*");
        public static void OnBlockBroken(IServerPlayer byPlayer, BlockSelection blockSel, ref float dropQuantityMultiplier, ref EnumHandling handling)
        {
            Block block = blockSel.Block;

            if (block == null) return;

            var data = byPlayer.Entity?.GetBehavior<EntityBehaviorPlayerMasteries>()?.PlayerMasteryData;
            data?.GainExperience(0);
        }

        private static BlockTypeEnum GetBlockType(AssetLocation block)
        {
            if (WildcardUtil.Match(block, oreWildcard))
            {
                return BlockTypeEnum.Ore;
            }else if (WildcardUtil.Match(block, cropWildcard))
            {
                return BlockTypeEnum.Crop;
            }else return BlockTypeEnum.NoXP;
        }

        private static float GetExpForBlock(Block block, IServerPlayer byPlayer)
        {
            switch (GetBlockType(block.Code))
            {
                case BlockTypeEnum.Ore:
                    return CalculateOreXP(block, byPlayer);
                case BlockTypeEnum.Crop:
                    return CalculateCropXP(block, byPlayer);
            }
            return 0f;
        }

        private static float CalculateOreXP(Block block, IServerPlayer byPlayer)
        {

        }

        private static float CalculateCropXP(Block block, IServerPlayer byPlayer)
        {

        }
    }
}
