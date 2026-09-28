using HarmonyLib;
using MasteryLibrary.src.Behaviors.EntityBehaviors;
using MasteryLibrary.src.Core.Masteries.Instances;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace DestariaMasteries.src.Patches
{
    [HarmonyPatch(typeof(BlockEntityFarmland), nameof(BlockEntityFarmland.TryPlant))]
    public static class BEFarmlandPatch_TryPlant
    {
        static void Postfix(
        bool __result,
        BlockEntityFarmland __instance,
        Block block,
        ItemSlot itemslot,
        EntityAgent byEntity,
        BlockSelection blockSel)
        {
            if (!__result) return;

            PlayerMasteryData? data = byEntity?.GetBehavior<EntityBehaviorPlayerMasteries>()?.PlayerMasteryData;
            bool hasSkill = data != null && data.HasSkill("GoldenFields");

            __instance.CropAttributes.SetBool("goldenFieldsBonus", hasSkill);
            __instance.MarkDirty(true);
        }

    }
    [HarmonyPatch(typeof(BlockEntityFarmland), nameof(BlockEntityFarmland.OnCropBlockBroken))]
    public static class BEFarmlandPatch_OnCropBlockBroken
    {
        static void Postfix(BlockEntityFarmland __instance)
        {
            __instance.CropAttributes.RemoveAttribute("goldenFieldsBonus"); // Remove the attribute so future crops dont get the bonus if they shoudnt have it
            __instance.CropAttributes.RemoveAttribute(ItemPlantableSeedPatch.FreeSeedAttr); // Same for the free-seed anti-dupe flag
        }
    }

    [HarmonyPatch(typeof(BlockCrop), nameof(BlockCrop.GetDrops), new Type[] { typeof(IWorldAccessor), typeof(BlockPos), typeof(IPlayer), typeof(float) })]
    public static class BlockCropPatch_GetDrops
    {
        static void Postfix(BlockCrop __instance, IWorldAccessor world, BlockPos pos, IPlayer byPlayer, ref ItemStack[] __result)
        {
            if (__result == null || __result.Length == 0) return;

            var cropProps = __instance.CropProps;
            if (cropProps == null) return;

            int.TryParse(__instance.LastCodePart(), out int stage);
            if (stage >= cropProps.GrowthStages)
            {
                PlayerMasteryData? data = byPlayer.Entity?.GetBehavior<EntityBehaviorPlayerMasteries>()?.PlayerMasteryData;
                bool hasSkill = data != null && data.HasSkill("EarlyBird");
                if (!hasSkill) return;
                double lastdayused = byPlayer.Entity?.WatchedAttributes.GetDouble("EarlyBirdLastActivated") ?? 0;
                double currentday = world.Calendar.TotalDays;
                byPlayer.Entity.Api.Logger.Debug($"Last Used: {lastdayused} \n Current Day: {currentday}");
                if ((int)currentday > (int)lastdayused) {

                    EntityBehaviorHealth? playerHealth = byPlayer.Entity?.GetBehavior<EntityBehaviorHealth>();
                    playerHealth?.Health = (float)Math.Min(playerHealth.MaxHealth, playerHealth.Health + playerHealth.MaxHealth * 0.5);

                    byPlayer.Entity?.WatchedAttributes.SetDouble("EarlyBirdLastActivated", currentday);
                }
                return;
            }

            if (!(world.BlockAccessor.GetBlockEntity(pos.DownCopy()) is BlockEntityFarmland beFarmland)) return;

            if (!beFarmland.CropAttributes.GetBool(ItemPlantableSeedPatch.FreeSeedAttr, false)) return;

            beFarmland.CropAttributes.RemoveAttribute(ItemPlantableSeedPatch.FreeSeedAttr);
            beFarmland.MarkDirty(true);

            __result = __result.Where(stack => !(stack?.Collectible is ItemPlantableSeed)).ToArray();
        }
    }
    [HarmonyPatch(typeof(BlockEntityFarmland), "updateCropDamage")]
    public static class BEFarmlandPatch_UpdateCropDamage
    {
        static void Prefix(
            BlockEntityFarmland __instance,
            double hourIntervall,
            Block cropBlock,
            bool hasCrop,
            bool hasRipeCrop,
            ClimateCondition conds,
            out float __state)
        {
            __state = float.NaN;

            if (!hasCrop || cropBlock?.CropProps == null || conds == null) return;
            if (!__instance.CropAttributes.GetBool("goldenFieldsBonus", false)) return;

            float currentTemp = conds.Temperature;
            float coldCap = cropBlock.CropProps.ColdDamageBelow;
            float heatCap = cropBlock.CropProps.HeatDamageAbove;

            float adjustedTemp = currentTemp;

            if (currentTemp < coldCap)
            {
                adjustedTemp = Math.Min(currentTemp + 5f, coldCap);
            }
            else if (currentTemp > heatCap)
            {
                adjustedTemp = Math.Max(currentTemp - 5f, heatCap);
            }

            if (adjustedTemp != currentTemp)
            {
                __state = currentTemp;
                conds.Temperature = adjustedTemp;
            }
        }

        static void Postfix(ClimateCondition conds, float __state)
        {
            if (conds != null && !float.IsNaN(__state))
            {
                conds.Temperature = __state; // Restore original temperature
            }
        }
    }
}