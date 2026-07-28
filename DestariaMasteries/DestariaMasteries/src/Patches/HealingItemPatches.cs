using HarmonyLib;
using System.Reflection;
using Vintagestory.API.Common;

namespace DestariaMasteries.src.Patches
{
    [HarmonyPatch]
    public class HealingItemPatchesOnHeldInteractStep
    {
        static bool Prepare()
        {
            return TargetMethod() != null;
        }

        static MethodBase? TargetMethod()
        {
            var type = AccessTools.TypeByName("Vintagestory.GameContent.CollectibleBehaviorHealingItem")
                ?? AccessTools.TypeByName("Vintagestory.GameContent.ItemBehaviorHealingItem");

            if (type == null) return null;

            var method = AccessTools.Method(type, "OnHeldInteractStep", new[]
            {
                typeof(float),
                typeof(ItemSlot),
                typeof(EntityAgent),
                typeof(BlockSelection),
                typeof(EntitySelection),
                typeof(EnumHandling).MakeByRefType()
            });

            return method?.DeclaringType == type ? method : null;
        }

        static void Prefix(ref float secondsUsed, EntityAgent byEntity)
        {
            float speedBonus = byEntity.Stats.GetBlended("healingItemUseSpeed") - 1f;
            if (speedBonus <= 0) return;

            secondsUsed *= 1f + speedBonus;
        }
    }
}
