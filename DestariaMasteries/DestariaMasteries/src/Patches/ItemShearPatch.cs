using DestariaMasteries.src.Utils;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace DestariaMasteries.src.Patches
{
    [HarmonyPatch(typeof(ItemShears), "breakMultiBlock")]
    public static class MultiBreakXpPatch
    {
        static void Prefix(BlockPos pos, IPlayer plr, out Block __state)
        {
            __state = plr?.Entity?.World.BlockAccessor.GetBlock(pos);
        }

        static void Postfix(BlockPos pos, IPlayer plr, Block __state)
        {
            if (__state == null || __state.Id == 0) return;
            if (plr is not IServerPlayer serverPlayer) return;

            var after = plr.Entity.World.BlockAccessor.GetBlock(pos);
            if (after.Id == __state.Id) return;

            XpRewardEvaluator.OnBlockBroken(serverPlayer, __state, pos);
        }
    }
}
