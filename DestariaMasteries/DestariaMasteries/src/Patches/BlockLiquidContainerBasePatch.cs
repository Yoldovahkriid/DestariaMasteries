using HarmonyLib;
using MasteryLibrary;
using MasteryLibrary.src.Behaviors.EntityBehaviors;
using MasteryLibrary.src.Core.Effects;
using MasteryLibrary.src.Core.Masteries.Instances;
using MasteryLibrary.src.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace DestariaMasteries.src.Patches
{
    internal class BlockLiquidContainerBasePatch
    {
    }
    [HarmonyPatch(typeof(BlockLiquidContainerBase))]
    [HarmonyPatch("tryEatStop", new Type[] { typeof(float), typeof(ItemSlot), typeof(EntityAgent) })]
    public static class BlockLiquidContainerBase_tryEatStop_JuicePatch
    {
        // Matches any domain, e.g. "game:juiceportion-blueberry"
        static readonly AssetLocation juiceWildcard = new AssetLocation("*:juiceportion-*");

        [HarmonyPrefix]
        static void Prefix(float secondsUsed, ItemSlot slot, EntityAgent byEntity, out string __state)
        {
            __state = null;

            if (secondsUsed < 0.95f) return;

            var containerStack = slot?.Itemstack;
            if (containerStack == null) return;

            var containerBlock = containerStack.Collectible as BlockLiquidContainerBase;
            if (containerBlock == null) return;

            ItemStack? contentStack = containerBlock.GetContent(containerStack);
            if (contentStack?.Collectible?.Code == null) return;

            if (WildcardUtil.Match(juiceWildcard, contentStack.Collectible.Code))
            {
                __state = contentStack.Collectible.Code.ToShortString();
            }
        }

        [HarmonyPostfix]
        static void Postfix(float secondsUsed, ItemSlot slot, EntityAgent byEntity, string __state)
        {
            if (__state == null) return;

            if (secondsUsed < 0.95f || byEntity.Api.Side != EnumAppSide.Server) return;

            if (byEntity.HasBehavior<EntityBehaviorEffects>() && byEntity.HasBehavior<EntityBehaviorPlayerMasteries>())
            {
                EntityBehaviorPlayerMasteries? masteries = byEntity.GetBehavior<EntityBehaviorPlayerMasteries>();
                SkillInstance? skill = masteries?.PlayerMasteryData.GetSkillInstance("BitFruity");

                if (skill == null || skill.Level != 5) return;

                EntityBehaviorEffects? agentEffects = byEntity.GetBehavior<EntityBehaviorEffects>();
                EffectManager? effectManager = agentEffects?.EffectManager;
                if (skill != null)
                {
                    MasteryLibraryAPI mapi = byEntity.Api.ModLoader.GetModSystem<MasteryLibraryAPI>();
                    EffectInstance? effect = mapi.EffectRegistry.CreateEffectInstance("StatBoost", byEntity.GetName(), (long)30 * 1000, skill.Level);
                    var activeStats = skill.Skill.GetValueSafe<Dictionary<string, StatConfiguration>>("ActiveStats");
                    if (activeStats != null)
                    {
                        effect?.CustomData.Add("ActiveStats", activeStats);
                    }
                    effectManager?.AddEffect(effect);
                }
            }
        }
    }
}
