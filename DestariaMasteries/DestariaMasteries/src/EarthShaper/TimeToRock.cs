using MasteryLibrary.src.Core.Abilities;
using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Util;

namespace DestariaMasteries.src.EarthShaper
{
    public class TimeToRock: Ability
    {
        public AssetLocation targetAsset = new AssetLocation("*:muddygravel");
        public override string Code => "TimeToRock";
        public override AbilityResult Execute(AbilityContext context)
        {
            BlockSelection blockSel = context.Player.CurrentBlockSelection;
            if (blockSel == null || blockSel.Block == null)
            {
                return AbilityResult.FailureResult("No block selected.");
            }
            if (WildcardUtil.Match(targetAsset, blockSel.Block.Code))
            {
                IPlayerInventoryManager inventoryManager = context.Player.InventoryManager;

                IWorldAccessor world = context.Player.Entity.World;
                Item? flint = world.GetItem(new AssetLocation("game:flint"));
                if (flint == null) return AbilityResult.FailureResult("Flint item code could not be resolved.");

                ItemStack flintStack = new ItemStack(flint, 1);
                if (!inventoryManager.TryGiveItemstack(flintStack, true))
                {
                    world.SpawnItemEntity(flintStack, context.Player.Entity.Pos.XYZ);
                }
                return AbilityResult.SuccessResult();
            }

            return AbilityResult.FailureResult("Selected block does not match the target asset.");
        }
    }
}
