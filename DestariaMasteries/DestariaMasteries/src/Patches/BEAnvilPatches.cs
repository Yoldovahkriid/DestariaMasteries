using DestariaMasteries.src.Utils;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace DestariaMasteries.src.Patches
{
    [HarmonyPatch(typeof(BlockEntityAnvil), nameof(BlockEntityAnvil.CheckIfFinished))]
    public static class AnvilSmithingXpPatch
    {
        public struct State
        {
            public SmithingRecipe? Recipe;
            public ItemStack? WorkItem;
        }

        [HarmonyPrefix]
        public static void Prefix(BlockEntityAnvil __instance, out State __state)
        {
            __state = default;

            if (__instance.Api?.Side != EnumAppSide.Server) return;

            __state.Recipe = __instance.SelectedRecipe;
            __state.WorkItem = __instance.WorkItemStack;
        }

        [HarmonyPostfix]
        public static void Postfix(BlockEntityAnvil __instance, IPlayer byPlayer, State __state)
        {
            if (__state.Recipe == null || __state.WorkItem == null) return;

            if (__instance.WorkItemStack != null) return;

            if (byPlayer is not IServerPlayer serverPlayer) return;

            XpRewardEvaluator.OnSmithingComplete(serverPlayer, __state.Recipe, __state.WorkItem);
        }
    }

}
