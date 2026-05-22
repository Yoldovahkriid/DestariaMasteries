using MasteryLibrary;
using MasteryLibrary.src.Behaviors.EntityBehaviors;
using MasteryLibrary.src.Core.Abilities;
using MasteryLibrary.src.Core.Effects;
using MasteryLibrary.src.Core.Masteries.Instances;
using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace DestariaMasteries.src.Alchemy
{
    public class DazzleBlast : Ability
    {
        public override string Code => "DazzleBlast";

        public override AbilityResult Execute(AbilityContext context)
        {
            IServerPlayer player = context.Player;
            EntityAgent? target = GetTargetedEntity(player);
            if (target == null) return AbilityResult.FailureResult("No valid target found.");

            EffectManager? manager = target.GetBehavior<EntityBehaviorEffects>()?.EffectManager;
            if (manager == null) return AbilityResult.FailureResult("Target cannot receive effects.");

            EffectRegistry register = context.API.ModLoader.GetModSystem<MasteryLibraryAPI>().EffectRegistry;
            EffectInstance? bInstance = register.CreateEffectInstance("Blindness", context.Player.PlayerName, 2 * context.Level * 1000, 1);

            if (bInstance == null) return AbilityResult.FailureResult();
            manager.AddEffect(bInstance);

            PlayerMasteryData? masteryData = player.Entity
                .GetBehavior<EntityBehaviorPlayerMasteries>()?.PlayerMasteryData;

            SkillInstance? concussive = masteryData?.GetSkillInstance("ConcussiveBlast");
            if (concussive != null)
            {
                EffectInstance? dInstance = register.CreateEffectInstance(
                    "Deafness",
                    context.Player.PlayerName,
                    concussive.Level * 2 * 1000,   // scales with ConcussiveBlast level
                    1
                );
                if (dInstance != null) manager.AddEffect(dInstance);
            }
            return AbilityResult.SuccessResult();
        }

        private EntityAgent? GetTargetedEntity(IServerPlayer player)
        {
            Vec3d eyepos = player.Entity.Pos.XYZ.AddCopy(0, player.Entity.LocalEyePos.Y, 0);
            BlockSelection blockSel = new BlockSelection();
            EntitySelection entitySel = new EntitySelection();

            player.Entity.Api.World.RayTraceForSelection(
                eyepos,
                player.Entity.Pos.Pitch,
                player.Entity.Pos.Yaw,
                10,
                ref blockSel,
                ref entitySel
            );

            if (entitySel?.Entity is EntityAgent targetAgent)
            {
                return targetAgent;
            }

            return null;
        }
    }
}
