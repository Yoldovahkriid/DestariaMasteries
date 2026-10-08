using HarmonyLib;
using MasteryLibrary;
using MasteryLibrary.src.Behaviors.EntityBehaviors;
using MasteryLibrary.src.Core.Effects;
using MasteryLibrary.src.Core.Masteries.Instances;
using MasteryLibrary.src.Utilities;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using Vintagestory.API.Common;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace DestariaMasteries.src.Patches
{
    [HarmonyPatch(typeof(CollectibleObject), "tryEatStop")]
    public static class CollectiblePatchesTryEatStopPatch
    {
        private static readonly AssetLocation mushroomWildcard = new AssetLocation("*:mushroom-*");
        private static readonly AssetLocation vegetableWildcard = new AssetLocation("*:vegetable-*");

        private static bool IsMushroom(AssetLocation asset) => WildcardUtil.Match(mushroomWildcard, asset);
        private static bool IsVegetable(AssetLocation asset) => WildcardUtil.Match(vegetableWildcard, asset);

        private static readonly System.Reflection.FieldInfo satietyField =
            AccessTools.Field(typeof(FoodNutritionProperties), nameof(FoodNutritionProperties.Satiety));
        private static readonly System.Reflection.FieldInfo healthField =
            AccessTools.Field(typeof(FoodNutritionProperties), nameof(FoodNutritionProperties.Health));

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            // 1) byEntity.ReceiveSaturation(nutriProps.Satiety * satLossMul, ...)
            //    IL: ldloc nutriProps; ldfld Satiety; ldloc satLossMul; mul
            //    -> after ldfld Satiety, run the value through AdjustSatiety(satiety, this, byEntity)
            matcher
                .MatchStartForward(
                    new CodeMatch(OpCodes.Ldfld, satietyField),
                    new CodeMatch(ci => ci.IsLdloc()),
                    new CodeMatch(OpCodes.Mul))
                .ThrowIfInvalid("tryEatStop transpiler: could not find Satiety * satLossMul")
                .Advance(1) // just past ldfld Satiety
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldarg_0),  // this (CollectibleObject)
                    new CodeInstruction(OpCodes.Ldarg_3),  // byEntity
                    CodeInstruction.Call(typeof(CollectiblePatchesTryEatStopPatch), nameof(AdjustSatiety)));

            // 2) float healthChange = nutriProps.Health * healthLossMul;
            //    IL: ldloc nutriProps; ldfld Health; ldloc healthLossMul; mul; stloc healthChange
            //    -> insert just before the stloc
            matcher
                .MatchEndForward(
                    new CodeMatch(OpCodes.Ldfld, healthField),
                    new CodeMatch(ci => ci.IsLdloc()),
                    new CodeMatch(OpCodes.Mul))
                .ThrowIfInvalid("tryEatStop transpiler: could not find Health * healthLossMul")
                .Advance(1) // now on the stloc
                .Insert(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldarg_3),
                    CodeInstruction.Call(typeof(CollectiblePatchesTryEatStopPatch), nameof(AdjustHealth)));

            return matcher.InstructionEnumeration();
        }

        public static float AdjustSatiety(float satiety, CollectibleObject collectible, EntityAgent byEntity)
        {
            float bonusNutrition = 0;

            if (IsMushroom(collectible.Code)
                && byEntity.HasBehavior<EntityBehaviorEffects>()
                && byEntity.HasBehavior<EntityBehaviorPlayerMasteries>())
            {
                bonusNutrition = byEntity.Stats.GetBlended("mushroomsSaturation");

                var masteries = byEntity.GetBehavior<EntityBehaviorPlayerMasteries>();
                SkillInstance? skill = masteries?.PlayerMasteryData.GetSkillInstance("FungalNourishment");
                EffectManager? effectManager = byEntity.GetBehavior<EntityBehaviorEffects>()?.EffectManager;

                if (skill != null)
                {
                    var mapi = byEntity.Api.ModLoader.GetModSystem<MasteryLibraryAPI>();
                    EffectInstance? effect = mapi.EffectRegistry.CreateEffectInstance(
                        "StatBoost", byEntity.GetName(), (long)skill.Skill.Duration * 1000, skill.Level);

                    var activeStats = skill.Skill.GetValueSafe<Dictionary<string, StatConfiguration>>("ActiveStats");
                    if (activeStats != null)
                    {
                        effect?.CustomData.Add("ActiveStats", activeStats);
                    }
                    if (effect != null) effectManager?.AddEffect(effect);
                }
            }

            if (IsVegetable(collectible.Code))
            {
                bonusNutrition = satiety * byEntity.Stats.GetBlended("vegetablesSaturation") - 1;
            }

            return satiety + bonusNutrition;
        }

        public static float AdjustHealth(float healthChange, CollectibleObject collectible, EntityAgent byEntity)
        {
            if (healthChange < 0 && IsMushroom(collectible.Code))
            {
                healthChange *= Math.Max(byEntity.Stats.GetBlended("mushroomDamageMult"), 0);
            }
            return healthChange;
        }
    }
}