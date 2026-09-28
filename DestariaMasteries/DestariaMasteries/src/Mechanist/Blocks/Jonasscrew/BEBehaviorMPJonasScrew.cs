using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent.Mechanics;
using static OpenTK.Graphics.OpenGL.GL;

namespace DestariaMasteries.src.Mechanist.Blocks.Jonasscrew
{
    public class BEBehaviorMPJonasScrew : BEBehaviorMPRotor
    {
        protected override float Resistance => 0.3f;
        protected override double AccelerationFactor => 1d;
        protected override float TargetSpeed => 0.1f;
        protected override float TorqueFactor => 0.5f;
        public BEBehaviorMPJonasScrew(BlockEntity blockentity) : base(blockentity)
        {

        }

        protected override CompositeShape GetShape()
        {
            CompositeShape shape = Block.Shape.Clone();
            shape.Base = new AssetLocation("destariamasteries:shapes/block/metal/jonasscrew-spinning.json");
            return shape;
        }

        public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tesselator)
        {
            base.OnTesselation(mesher, tesselator);

            ICoreClientAPI capi = Api as ICoreClientAPI;
            Shape shape = Vintagestory.API.Common.Shape.TryGet(capi, "destariamasteries:shapes/block/metal/jonasscrew-hull.json");
            float rotateY = 0f;
            switch (BlockFacing.FromCode(Block.Variant["side"]).Index)
            {
                case 0:
                    AxisSign = new int[] { 0, 0, -1 };
                    rotateY = 180;
                    break;
                case 1:
                    AxisSign = new int[] { -1, 0, 0 };
                    rotateY = 90;
                    break;
                case 3:
                    AxisSign = new int[] { -1, 0, 0 };
                    rotateY = 270;
                    break;
                default:
                    break;
            }
            capi.Tesselator.TesselateShape(Block, shape, out MeshData mesh, new Vec3f(0, rotateY, 0));
            mesher.AddMeshData(mesh);
            return true;
        }
    }
}
