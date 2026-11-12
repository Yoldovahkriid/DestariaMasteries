using System;
using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace DestariaMasteries.src.Behavior.CollectibleBehaviors
{
    public class CollectibleBehaviorStatBasedDurabilityScale : CollectibleBehavior
    {
        public const string BuffCode = "statdurability";

        public string StatCode = "stoneToolDurabilityMul";

        public string[] allowedVariants = Array.Empty<string>(); // List of allowed item variants. (Only really needed if using BehaviorsByType is not applicable)
        public CollectibleBehaviorStatBasedDurabilityScale(CollectibleObject collObj) : base(collObj)
        {
        }

        public override void Initialize(JsonObject properties)
        {
            base.Initialize(properties);

            if (properties?["allowedVariants"] != null)
            {
                allowedVariants = properties["allowedVariants"].AsArray<string>(Array.Empty<string>());
            }
        }

        public override void OnCreatedByCrafting(ItemSlot[] allInputSlots, ItemSlot outputSlot, IRecipeBase byRecipe, ref EnumHandling bhHandling)
        {
            var stack = outputSlot?.Itemstack;
            if (stack == null) return;

            if (!IsVariantAllowed(stack)) return;

            var buffable = collObj.GetBehavior<CollectibleBehaviorBuffable>();
            if (buffable == null) return;

            var ent = GetCrafter(allInputSlots, outputSlot)?.Entity;
            if (ent == null) return;

            float bonus = ent.Stats.GetBlended(StatCode) - 1f;
            if (bonus <= 0f) return;

            buffable.AddBuff(stack, new AppliedCollectibleBuff()
            {
                Code = BuffCode,
                StatCode = "maxdurability",
                Multiplier = 1f + bonus,
                RemainingDurability = -1
            }, EnumBuffAddType.ReplaceOnDuplicate);
        }

        private bool IsVariantAllowed(ItemStack stack)
        {
            if (allowedVariants == null || allowedVariants.Length == 0) return true;

            string itemCode = stack.Collectible.Code?.Path ?? "";
            string fullCode = stack.Collectible.Code?.ToString() ?? "";

            foreach (var pattern in allowedVariants)
            {
                if (WildcardUtil.Match(pattern, itemCode) || WildcardUtil.Match(pattern, fullCode))
                {
                    return true;
                }
            }

            return false;
        }

        private static IPlayer? GetCrafter(ItemSlot[] allInputSlots, ItemSlot outputSlot)
        {
            var player = (outputSlot.Inventory as InventoryBasePlayer)?.Player;
            if (player != null) return player;

            foreach (var slot in allInputSlots)
            {
                player = (slot?.Inventory as InventoryBasePlayer)?.Player;
                if (player != null) return player;
            }
            return null;
        }
    }
}