using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent.Mechanics;

namespace DestariaMasteries.src.Mechanist.Blocks.Jonasscrew
{
    public class BlockJonasScrew : BlockMPBase, IMPPowered
    {
        BlockFacing powerOutFacing;

        public override void OnLoaded(ICoreAPI api)
        {
            powerOutFacing = BlockFacing.FromCode(Variant["side"]).Opposite;

            base.OnLoaded(api);
        }
        public override void DidConnectAt(IWorldAccessor world, BlockPos pos, BlockFacing face)
        {

        }

        public override bool HasMechPowerConnectorAt(IWorldAccessor world, BlockPos pos, BlockFacing face, BlockMPBase forBlock)
        {
            return face == powerOutFacing;
        }

        public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel, ref string failureCode)
        {
            if (!CanPlaceBlock(world, byPlayer, blockSel, ref failureCode))
            {
                return false;
            }

            foreach (BlockFacing face in BlockFacing.HORIZONTALS)
            {
                BlockPos pos = blockSel.Position.AddCopy(face);
                IMechanicalPowerBlock block = world.BlockAccessor.GetBlock(pos) as IMechanicalPowerBlock;
                if (block != null)
                {
                    if (block.HasMechPowerConnectorAt(world, pos, face.Opposite, this))
                    {
                        if (block is IMPPowered) return false;

                        Block toPlaceBlock = world.GetBlock(new AssetLocation(Code.Domain, FirstCodePart() + "-" + face.Opposite.Code));
                        world.BlockAccessor.SetBlock(toPlaceBlock.BlockId, blockSel.Position);

                        block.DidConnectAt(world, pos, face.Opposite);
                        WasPlaced(world, blockSel.Position, face);

                        return true;
                    }
                }
            }

            bool ok = base.TryPlaceBlock(world, byPlayer, itemstack, blockSel, ref failureCode);
            if (ok)
            {
                WasPlaced(world, blockSel.Position, null);
            }
            return ok;
        }

    }
}
