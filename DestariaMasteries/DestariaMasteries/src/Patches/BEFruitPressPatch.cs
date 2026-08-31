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
    [HarmonyPatch(typeof(BlockEntityFruitPress), "OnBlockInteractStop")]
    public static class BEFruitPressPatch
    {
        static readonly PropertyInfo animUtilProp = AccessTools.Property(typeof(BlockEntityFruitPress), "animUtil");
        static readonly FieldInfo compressMetaFld = AccessTools.Field(typeof(BlockEntityFruitPress), "compressAnimMeta");
        static readonly MethodInfo updateSqueezeMi = AccessTools.Method(typeof(BlockEntityFruitPress), "updateSqueezeRel");
        static readonly FieldInfo packetIdAnimFld = AccessTools.Field(typeof(BlockEntityFruitPress), "PacketIdAnimUpdate");
        static readonly int packetIdAnimUpdate = (int)packetIdAnimFld.GetValue(null);

        static bool Prefix(BlockEntityFruitPress __instance, IPlayer byPlayer)
        {
            if (!__instance.CompressAnimActive) return true;

            EntityAgent byEntity = byPlayer?.Entity;
            PlayerMasteryData? data = byEntity?.GetBehavior<EntityBehaviorPlayerMasteries>()?.PlayerMasteryData;
            bool hasSkill = data != null && data.HasSkill("MacroBrewery");

            if (!hasSkill) return true;

            var animUtil = (BlockEntityAnimationUtil)animUtilProp.GetValue(__instance);
            RunningAnimation anim = animUtil?.animator?.GetAnimationState("compress");
            if (anim == null) return true;

            anim.CurrentFrame = anim.Animation.QuantityFrames - 1;

            updateSqueezeMi.Invoke(__instance, new object[] { anim });

            var compressAnimMeta = (AnimationMetaData)compressMetaFld.GetValue(__instance);
            compressAnimMeta.AnimationSpeed = 0f;

            (__instance.Api as ICoreServerAPI)?.Network.BroadcastBlockEntityPacket(
                __instance.Pos,
                packetIdAnimUpdate,
                new FruitPressAnimPacket
                {
                    AnimationState = EnumFruitPressAnimState.ScrewContinue,
                    AnimationSpeed = 0f,
                    CurrentFrame = anim.CurrentFrame
                }
            );

            __instance.MarkDirty(true);

            return false;
        }
    }

    [HarmonyPatch(typeof(BlockEntityFruitPress), nameof(BlockEntityFruitPress.OnBlockInteractStart))]
    public static class BEFruitPressPatch_OnBlockInteractStart
    {
        static void Prefix(BlockEntityFruitPress __instance, IPlayer byPlayer, ref int __state)
        {
            ItemSlot? slot = byPlayer?.InventoryManager?.ActiveHotbarSlot;
            __state = slot?.Itemstack != null ? slot.Itemstack.StackSize : 0;
        }

        static void Postfix(BlockEntityFruitPress __instance, IPlayer byPlayer, int __state)
        {
            if (byPlayer == null || __state <= 0) return;

            ItemSlot slot = byPlayer.InventoryManager.ActiveHotbarSlot;
            if (slot?.Itemstack == null) return;

            int currentSize = slot.Itemstack.StackSize;
            if (currentSize >= __state) return;

            float saveChance = byPlayer.Entity.Stats.GetBlended("fruitPressSaveChance") - 1;

            for (int i = 0; i < __state - currentSize; i++)
            {
                if (byPlayer.Entity.World.Rand.NextDouble() < saveChance)
                {
                    slot.Itemstack.StackSize++;
                    slot.MarkDirty();
                }
            }
        }
    }
}
