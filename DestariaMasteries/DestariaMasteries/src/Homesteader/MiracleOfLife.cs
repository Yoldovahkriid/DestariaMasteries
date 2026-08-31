using MasteryLibrary.src.Core.Abilities;
using System;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace DestariaMasteries.src.Homesteader
{
    public class MiracleOfLife : Ability
    {
        private const float TargetRange = 10f;

        public override string Code => "MiracleOfLife";

        public override AbilityResult CanUse(AbilityContext context)
        {
            Entity target = GetTargetEntity(context);

            if (target == null || !target.Alive)
            {
                return AbilityResult.FailureResult("You must be looking at a living animal to use this ability.");
            }

            EntityBehaviorMultiply multiplyBehavior = target.GetBehavior<EntityBehaviorMultiply>();
            if (multiplyBehavior == null)
            {
                return AbilityResult.FailureResult("That animal is not capable of giving birth.");
            }

            if (!multiplyBehavior.IsPregnant)
            {
                return AbilityResult.FailureResult("That animal isn't pregnant.");
            }

            return AbilityResult.SuccessResult();
        }

        public override AbilityResult Execute(AbilityContext context)
        {
            ICoreServerAPI api = context.API;

            Entity target = GetTargetEntity(context);
            if (target == null || !target.Alive)
            {
                return AbilityResult.FailureResult("You must be looking at a living animal to use this ability.");
            }

            EntityBehaviorMultiply multiplyBehavior = target.GetBehavior<EntityBehaviorMultiply>();
            if (multiplyBehavior == null || !multiplyBehavior.IsPregnant)
            {
                return AbilityResult.FailureResult("That animal isn't pregnant.");
            }

            int offspringCount = InduceBirth(api, multiplyBehavior);

            return AbilityResult.SuccessResult(
                $"You have used the Miracle of Life ability! {target.GetName()} gives birth.",
                offspringCount
            );
        }

        private Entity GetTargetEntity(AbilityContext context)
        {
            Entity customTarget = context.GetValueSafe<Entity>("targetEntity");
            if (customTarget != null)
            {
                return customTarget;
            }

            EntitySelection entitySel = RaycastForEntity(context);
            return entitySel?.Entity;
        }

        private EntitySelection RaycastForEntity(AbilityContext context)
        {
            Entity playerEntity = context.Player.Entity;

            Vec3d eyePos = playerEntity.Pos.XYZ.AddCopy(0, playerEntity.LocalEyePos.Y, 0);
            BlockSelection blockSel = new BlockSelection();
            EntitySelection entitySel = new EntitySelection();

            playerEntity.Api.World.RayTraceForSelection(
                eyePos,
                playerEntity.Pos.Pitch,
                playerEntity.Pos.Yaw,
                TargetRange,
                ref blockSel,
                ref entitySel
            );

            return entitySel;
        }

        private int InduceBirth(ICoreServerAPI api, EntityBehaviorMultiply multiplyBehavior)
        {
            Random rand = api.World.Rand;
            double daysNow = api.World.Calendar.TotalDays;

            float rolledQuantity = multiplyBehavior.SpawnQuantityMin
                + (float)rand.NextDouble() * (multiplyBehavior.SpawnQuantityMax - multiplyBehavior.SpawnQuantityMin);

            int quantity = Math.Max(1, (int)Math.Round(rolledQuantity));

            multiplyBehavior.TestCommand(quantity);

            multiplyBehavior.TotalDaysLastBirth = daysNow;
            multiplyBehavior.TotalDaysCooldownUntil = daysNow
                + (multiplyBehavior.MultiplyCooldownDaysMin
                   + rand.NextDouble() * (multiplyBehavior.MultiplyCooldownDaysMax - multiplyBehavior.MultiplyCooldownDaysMin));
            multiplyBehavior.IsPregnant = false;

            return quantity;
        }
    }
}