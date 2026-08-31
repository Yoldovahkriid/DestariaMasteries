using MasteryLibrary.src.Core.Abilities;
using MasteryLibrary.src.Utilities;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace DestariaMasteries.src.Homesteader
{
    public class GentleTouch : Ability
    {
        public override string Code => "GentleTouch";

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

            if (entitySel == null || entitySel.Entity == null)
            {
                return AbilityResult.FailureResult("No valid entity selected.");
            }

            EntityBehaviorMilkable milkableBehavior = entitySel.Entity.GetBehavior<EntityBehaviorMilkable>();
            if (milkableBehavior == null)
            {
                return AbilityResult.FailureResult("Selected entity is not milkable.");
            }
            Dictionary<string, StatConfiguration> activeStats = context.GetValueSafe<Dictionary<string, StatConfiguration>>("ActiveStats");
            StatConfiguration reviveconfig = activeStats.GetValueOrDefault("calmChance", null);
            float chance = StatScalingUtil.GetScaledValue(reviveconfig, context.Level);

            if (context.API.World.Rand.NextDouble() < chance)
            {
                PropertyInfo lastMilkedTotalHours = typeof(EntityBehaviorMilkable).GetProperty("lastMilkedTotalHours", BindingFlags.NonPublic | BindingFlags.Instance);
                lastMilkedTotalHours.SetValue(milkableBehavior, (double)lastMilkedTotalHours.GetValue(milkableBehavior) - 24);
                return AbilityResult.FailureResult("Entity was calmed."); // Return failure to not start the cooldown. (Cooldown only applied if ability fails)
            }

            return AbilityResult.SuccessResult();
        }
    }
}
