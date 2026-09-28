using MasteryLibrary.src.Core.Abilities;
using MasteryLibrary.src.Utilities;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;
using static OpenTK.Graphics.OpenGL.GL;
using static Vintagestory.Server.Timer;

namespace DestariaMasteries.src.Homesteader
{
    public class GroveTending : Ability
    {
        public override string Code => "GroveTending";  
        public override AbilityResult Execute(AbilityContext context)
        {
            Vec3d eyepos = context.Player.Entity.Pos.XYZ.AddCopy(0, context.Player.Entity.LocalEyePos.Y, 0);
            BlockSelection blockSel = new BlockSelection();
            EntitySelection entitySel = new EntitySelection();

            context.Player.Entity.Api.World.RayTraceForSelection(
                eyepos,
                context.Player.Entity.Pos.Pitch,
                context.Player.Entity.Pos.Yaw,
                10,
                ref blockSel,
                ref entitySel
            );

            Dictionary<string, StatConfiguration> activeStats = context.GetValueSafe<Dictionary<string, StatConfiguration>>("ActiveStats");
            StatConfiguration reviveconfig = activeStats.GetValueOrDefault("cuttingReviveChance", null);
            float chance = StatScalingUtil.GetScaledValue(reviveconfig, context.Level);

            if (blockSel == null || blockSel.Position == null)
            {
                return AbilityResult.FailureResult("No valid block selected.");
            }

            Block block = context.Player.Entity.Api.World.BlockAccessor.GetBlock(blockSel.Position);
            if (block.Code.Path == "fruittree-cutting")
            {
                if (context.API.World.Rand.NextDouble() <= chance)
                {
                    FruitTreeGrowingBranchBH fruittree = block.GetBEBehavior<FruitTreeGrowingBranchBH>(blockSel.Position);
                    BlockEntityFruitTreeBranch branchEntity = fruittree.Blockentity as BlockEntityFruitTreeBranch;
                    branchEntity.FoliageState = EnumFoliageState.Plain;
                    FieldInfo listenerIdfield = fruittree.GetType().GetField("listenerId", BindingFlags.NonPublic | BindingFlags.Instance);
                    FieldInfo callbackTimeMs = fruittree.GetType().GetField("callbackTimeMs", BindingFlags.NonPublic | BindingFlags.Instance);
                    MethodInfo onTickMethod = fruittree.GetType().GetMethod("OnTick", BindingFlags.NonPublic | BindingFlags.Instance);
                    var DelegateAction = (Action<float>)onTickMethod.CreateDelegate(typeof(Action<float>), fruittree);
                    if (listenerIdfield != null)
                    {
                        long listenerId = (long)listenerIdfield.GetValue(fruittree);
                        listenerIdfield.SetValue(fruittree, branchEntity.RegisterGameTickListener(DelegateAction, (int)callbackTimeMs.GetValue(fruittree) + branchEntity.Api.World.Rand.Next((int)callbackTimeMs.GetValue(fruittree))));
                    }
                    branchEntity.GrowTries = 1;
                    var rootBe = context.Player.Entity.Api.World.BlockAccessor.GetBlockEntity(branchEntity.Pos.AddCopy(branchEntity.RootOff)) as BlockEntityFruitTreeBranch;
                    var rootbh = rootBe.GetBehavior<FruitTreeRootBH>();
                    rootbh.propsByType[branchEntity.TreeType].State = EnumFruitTreeState.Young;
                    branchEntity.MarkDirty(true);
                }
                else
                {
                    context.API.World.BlockAccessor.BreakBlock(blockSel.Position, context.Player);
                }
            }

            return AbilityResult.SuccessResult("Grove Tending executed successfully.");
        }
    }
}
