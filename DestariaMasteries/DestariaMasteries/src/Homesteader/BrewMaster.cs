using MasteryLibrary.src.Core.Abilities;
using System;
using System.Linq;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace DestariaMasteries.src.Homesteader
{
    public class BrewMaster : Ability
    {
        public override string Code => "BrewMaster";
        private const double BackdateHours = 100000;

        public override AbilityResult CanUse(AbilityContext context)
        {
            var barrel = GetTargetedSealedBarrel(context, out string error);
            if (barrel == null)
            {
                return AbilityResult.FailureResult(error);
            }

            if (FindTemporalGearSlot(context.Player) == null)
            {
                return AbilityResult.FailureResult("You need a temporal gear to do this.");
            }

            return AbilityResult.SuccessResult();
        }

        public override AbilityResult Execute(AbilityContext context)
        {
            var barrel = GetTargetedSealedBarrel(context, out string error);
            if (barrel == null)
            {
                return AbilityResult.FailureResult(error);
            }

            var gearSlot = FindTemporalGearSlot(context.Player);
            if (gearSlot == null)
            {
                return AbilityResult.FailureResult("You need a temporal gear to do this.");
            }

            gearSlot.TakeOut(1);
            gearSlot.MarkDirty();

            barrel.SealedSinceTotalHours = context.API.World.Calendar.TotalHours - BackdateHours;
            barrel.MarkDirty(true);

            return AbilityResult.SuccessResult(
                "You crush the temporal gear against the barrel, and its contents shimmer and finish aging."
            );
        }

        private BlockEntityBarrel GetTargetedSealedBarrel(AbilityContext context, out string error)
        {
            error = "";
            var blockSel = context.Player.CurrentBlockSelection;
            if (blockSel == null)
            {
                error = "You need to be looking at a sealed barrel.";
                return null;
            }

            var be = context.API.World.BlockAccessor.GetBlockEntity(blockSel.Position) as BlockEntityBarrel;
            if (be == null)
            {
                error = "You need to be looking at a barrel.";
                return null;
            }

            if (!be.Sealed)
            {
                error = "The barrel needs to be closed/sealed first.";
                return null;
            }

            return be;
        }

        private ItemSlot FindTemporalGearSlot(IServerPlayer player)
        {
            foreach (var inv in new[]
            {
                player.InventoryManager.GetOwnInventory(GlobalConstants.hotBarInvClassName),
                player.InventoryManager.GetOwnInventory(GlobalConstants.backpackInvClassName)
            })
            {
                if (inv == null) continue;

                var slot = inv.FirstOrDefault(s =>
                    s.Itemstack != null && s.Itemstack.Collectible.Code.Path == "gear-temporal");

                if (slot != null) return slot;
            }

            return null;
        }
    }
}