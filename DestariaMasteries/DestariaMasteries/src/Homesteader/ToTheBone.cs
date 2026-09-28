using MasteryLibrary.src.Core.Abilities;
using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace DestariaMasteries.src.Homesteader
{
    public class ToTheBone: Ability
    {
        public override string Code => "ToTheBone";
        private const string toTheBoneUsesKey = "destariamasteries:tothebone-uses";
        private const string toTheBoneLastUsedKey = "destariamasteries:tothebone-lastusedayutc";

        private static long GetUtcDayIndex()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds() / 86400L;
        }

        private static bool ToTheBoneUsedToday(EntityPlayer player)
        {
            long lastUseDay = player.WatchedAttributes.GetLong(toTheBoneLastUsedKey, -1L);
            return lastUseDay == GetUtcDayIndex();
        }
        private static void MarkToTheBoneUsed(EntityPlayer player)
        {
            player.WatchedAttributes.SetLong(toTheBoneLastUsedKey, GetUtcDayIndex());
        }

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

            if (entitySel.Entity.Alive)
            {
                return AbilityResult.FailureResult("Selected entity is not dead.");
            }

            if (entitySel.Entity.WatchedAttributes.HasAttribute("ToTheBone") || entitySel.Entity.WatchedAttributes.GetBool("ToTheBone") == true)
            {
                return AbilityResult.FailureResult("Selected entity has already been targeted by To The Bone.");
            }

            if (ToTheBoneUsedToday(context.Player.Entity))
            {
                if (context.Player.Entity.WatchedAttributes.GetInt(toTheBoneUsesKey, 0) < context.Level)
                {
                    entitySel.Entity.WatchedAttributes.SetBool("ToTheBone", true);
                    context.Player.Entity.WatchedAttributes.SetInt(toTheBoneUsesKey, context.Player.Entity.WatchedAttributes.GetInt(toTheBoneUsesKey, 0) + 1);
                    MarkToTheBoneUsed(context.Player.Entity);
                    return AbilityResult.SuccessResult();
                }
                return AbilityResult.FailureResult("You have already used all your to The Bone uses today.");
            }
            else
            {
                entitySel.Entity.WatchedAttributes.SetBool("ToTheBone", true);
                MarkToTheBoneUsed(context.Player.Entity);
                context.Player.Entity.WatchedAttributes.SetInt(toTheBoneUsesKey, 1);
            }

            return AbilityResult.SuccessResult();
        }
    }
}
