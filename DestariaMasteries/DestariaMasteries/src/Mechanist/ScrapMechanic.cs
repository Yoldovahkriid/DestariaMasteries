using MasteryLibrary.src.Core.Abilities;
using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.Common;

namespace DestariaMasteries.src.Mechanist
{
    public class ScrapMechanic : Ability
    {
        public override string Code => "ScrapMechanic";

        public override AbilityResult Execute(AbilityContext context)
        {
            IPlayerInventoryManager inventoryManager = context.Player.InventoryManager;
            ItemSlot activeSlot = inventoryManager.ActiveHotbarSlot;
            ItemStack? itemStack = activeSlot.Itemstack;

            if (itemStack == null ) return AbilityResult.FailureResult("No item in active hotbar slot.");
            if (itemStack.Collectible.Code.Path != "metal-scraps") return AbilityResult.FailureResult("Item in active hotbar slot is not metal scrap.");

            IWorldAccessor world = context.Player.Entity.World;
            Item? gear = world.GetItem(new AssetLocation("game:gear-rusty"));
            if (gear == null) return AbilityResult.FailureResult("Gear item code could not be resolved.");

            int amountToExchange = Math.Min(itemStack.StackSize, context.Level);
            activeSlot.TakeOut(amountToExchange);
            activeSlot.MarkDirty();

            ItemStack gearStack = new ItemStack(gear, amountToExchange);
            if (!inventoryManager.TryGiveItemstack(gearStack, true))
            {
                world.SpawnItemEntity(gearStack, context.Player.Entity.Pos.XYZ);
            }

            return AbilityResult.SuccessResult();
        }
    }
}
