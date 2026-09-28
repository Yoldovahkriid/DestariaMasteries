using CombatOverhaul.Implementations;
using DestariaMasteries.src.Utils;
using HarmonyLib;
using MasteryLibrary.src.Behaviors.EntityBehaviors;
using MasteryLibrary.src.Core.Masteries.Instances;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
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

    [HarmonyPatch(typeof(BEBehaviorFruitingBush), nameof(BEBehaviorFruitingBush.OnBlockInteractStop))]
    public static class BEBehaviorFruitingBush_OnBlockInteractStopPatch
    {
        private static readonly FieldInfo BhBushField = AccessTools.Field(typeof(BEBehaviorFruitingBush), "bhBush");
        private static readonly MethodInfo GetHarvestTimeMulMethod = AccessTools.Method(typeof(BEBehaviorFruitingBush), "getHarvestTimeMul");
        public static void Prefix(BEBehaviorFruitingBush __instance, out bool __state)
        {
            __state = __instance.BState != null && __instance.BState.Growthstate == EnumFruitingBushGrowthState.Ripe;
        }

        public static void Postfix(BEBehaviorFruitingBush __instance, bool __state, IWorldAccessor world, IPlayer byPlayer)
        {
            if (world.Side != EnumAppSide.Server || byPlayer == null) return;

            if (__state && __instance.BState.Growthstate == EnumFruitingBushGrowthState.Mature)
            {
                XpRewardEvaluator.OnHarvest(byPlayer as IServerPlayer, 5f);
            }
        }
    }
}
