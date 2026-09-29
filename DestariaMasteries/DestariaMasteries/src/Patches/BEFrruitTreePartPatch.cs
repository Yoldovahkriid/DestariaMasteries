using DestariaMasteries.src.Utils;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace DestariaMasteries.src.Patches
{
    [HarmonyPatch(typeof(BlockEntityFruitTreePart), nameof(BlockEntityFruitTreePart.OnBlockInteractStop))]
    public static class BEFrruitTreePartPatch
    {
        public static void Prefix(BlockEntityFruitTreePart __instance, out bool __state)
        {
            __state = __instance.FoliageState == EnumFoliageState.Ripe;
        }

        public static void Postfix(BlockEntityFruitTreePart __instance, bool __state, float secondsUsed, IPlayer byPlayer, BlockSelection blockSel)
        {
            if (__instance.Api.Side != EnumAppSide.Server || byPlayer == null) return;

            if (__state && __instance.FoliageState == EnumFoliageState.Plain)
            {
                XpRewardEvaluator.OnHarvest(byPlayer as IServerPlayer, 5f, blockSel.Position, __instance.Block.Code);
            }
        }
    }
}
