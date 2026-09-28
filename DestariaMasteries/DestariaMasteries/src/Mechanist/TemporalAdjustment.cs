using MasteryLibrary.src.Core.Abilities;
using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Common;

namespace DestariaMasteries.src.Mechanist
{
    public class TemporalAdjustment : Ability
    {
        public override string Code => "TemporalAdjustment";
        //Honestly more or less of a copy of ScrapMechanic
        public override AbilityResult Execute(AbilityContext context)
        {
            IPlayerInventoryManager inventoryManager = context.Player.InventoryManager;
            ItemSlot activeSlot = inventoryManager.ActiveHotbarSlot;
            ItemStack? itemStack = activeSlot.Itemstack;

            if (itemStack == null) return AbilityResult.FailureResult("No item in active hotbar slot.");
            if (itemStack.Collectible.Code.Path != "gear-rusty") return AbilityResult.FailureResult("Item in active hotbar slot is not metal scrap.");

            IWorldAccessor world = context.Player.Entity.World;
            Item? gear = world.GetItem(new AssetLocation("game:gear-temporal"));
            if (gear == null) return AbilityResult.FailureResult("Gear item code could not be resolved.");

            int amountToExchange = Math.Min(itemStack.StackSize, context.Level);
            activeSlot.TakeOut(amountToExchange);
            activeSlot.MarkDirty();

            ItemStack gearStack = new ItemStack(gear, amountToExchange);
            if (!inventoryManager.TryGiveItemstack(gearStack, true))
            {
                world.SpawnItemEntity(gearStack, context.Player.Entity.Pos.XYZ);
            }
            // We give the player the temporal gear but only then deal damage to him this should avoid lethal damage causing the gear to teleport to the player
            context.Player.Entity.ReceiveDamage(new DamageSource() { Source = EnumDamageSource.Internal, Type = EnumDamageType.Injury, IgnoreInvFrames = true }, 7.5f);

            return AbilityResult.SuccessResult();
        }
    }
}
