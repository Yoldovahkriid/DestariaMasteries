using MasteryLibrary.src.Core.Abilities;
using System;
using System.Reflection;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace DestariaMasteries.src.Healing
{
    public class ShockingFingers : Ability
    {
        private const float ReviveRange = 5f;
        private const double MaxAimAngleDegrees = 35;

        public override string Code => "ShockingFingers";

        public override AbilityResult CanUse(AbilityContext context)
        {
            EntityPlayer? target = GetTargetedRevivablePlayer(context.Player);
            if (target == null) return AbilityResult.FailureResult("No revivable player in range.");

            return AbilityResult.SuccessResult();
        }

        public override AbilityResult Execute(AbilityContext context)
        {
            EntityPlayer? target = GetTargetedRevivablePlayer(context.Player);
            if (target == null) return AbilityResult.FailureResult("No revivable player in range.");

            EntityBehavior? revivable = target.GetBehavior("playerrevivable");
            if (revivable == null) return AbilityResult.FailureResult("Target cannot be revived.");

            if (!CanRevive(revivable)) return AbilityResult.FailureResult("Target cannot be revived yet.");

            bool revived = TryAttemptRevive(revivable, context.Player, target);
            if (!revived) return AbilityResult.FailureResult("Revive failed.");

            SpawnReviveParticles(target);
            return AbilityResult.SuccessResult();
        }

        private EntityPlayer? GetTargetedRevivablePlayer(IServerPlayer player)
        {
            Vec3d eyepos = player.Entity.Pos.XYZ.AddCopy(0, player.Entity.LocalEyePos.Y, 0);
            BlockSelection blockSel = new BlockSelection();
            EntitySelection entitySel = new EntitySelection();

            player.Entity.Api.World.RayTraceForSelection(
                eyepos,
                player.Entity.Pos.Pitch,
                player.Entity.Pos.Yaw,
                ReviveRange,
                ref blockSel,
                ref entitySel
            );

            if (entitySel?.Entity is EntityPlayer targetedPlayer && IsRevivable(targetedPlayer))
            {
                return targetedPlayer;
            }

            return GetNearestAimedRevivablePlayer(player, eyepos);
        }

        private EntityPlayer? GetNearestAimedRevivablePlayer(IServerPlayer player, Vec3d eyepos)
        {
            Vec3f viewVector = player.Entity.Pos.GetViewVector();
            Vec3d lookVec = new Vec3d(viewVector.X, viewVector.Y, viewVector.Z).Normalize();
            EntityPlayer? bestTarget = null;
            double bestDistance = double.MaxValue;
            double minDot = Math.Cos(MaxAimAngleDegrees * GameMath.DEG2RAD);

            Entity[] nearbyEntities = player.Entity.World.GetEntitiesAround(
                player.Entity.Pos.XYZ,
                ReviveRange,
                ReviveRange,
                entity => entity is EntityPlayer && entity.EntityId != player.Entity.EntityId
            );

            foreach (Entity entity in nearbyEntities)
            {
                if (entity is not EntityPlayer candidate) continue;
                if (!IsRevivable(candidate)) continue;

                Vec3d toCandidate = candidate.Pos.XYZ.AddCopy(0, candidate.LocalEyePos.Y, 0).Sub(eyepos);
                double distance = toCandidate.Length();
                if (distance <= 0 || distance > ReviveRange) continue;

                double dot = lookVec.Dot(toCandidate.Normalize());
                if (dot < minDot || distance >= bestDistance) continue;

                bestTarget = candidate;
                bestDistance = distance;
            }

            return bestTarget;
        }

        private bool IsRevivable(EntityPlayer player)
        {
            EntityBehavior? revivable = player.GetBehavior("playerrevivable");
            return revivable != null && CanRevive(revivable);
        }

        private bool CanRevive(EntityBehavior revivable)
        {
            PropertyInfo? property = revivable.GetType().GetProperty(
                "CanRevive",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
            );

            return property?.GetValue(revivable) is bool canRevive && canRevive;
        }

        private bool TryAttemptRevive(EntityBehavior revivable, IServerPlayer caster, EntityPlayer target)
        {
            MethodInfo[] methods = revivable.GetType().GetMethods(
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
            );

            foreach (MethodInfo method in methods)
            {
                if (method.Name != "AttemptRevive") continue;

                if (!TryBuildArguments(method, caster, target, out object?[] arguments)) continue;

                try
                {
                    object? result = method.Invoke(revivable, arguments);
                    return result is not bool boolResult || boolResult;
                }
                catch
                {
                    // Try the next overload, if Vintage Story changes the signature.
                }
            }

            return false;
        }

        private bool TryBuildArguments(MethodInfo method, IServerPlayer caster, EntityPlayer target, out object?[] arguments)
        {
            ParameterInfo[] parameters = method.GetParameters();
            arguments = new object?[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                Type parameterType = parameters[i].ParameterType;
                string parameterName = parameters[i].Name ?? string.Empty;

                if (parameterName.Contains("target", StringComparison.OrdinalIgnoreCase)
                    && parameterType.IsAssignableFrom(target.GetType()))
                {
                    arguments[i] = target;
                }
                else if (parameterType.IsAssignableFrom(caster.GetType()))
                {
                    arguments[i] = caster;
                }
                else if (parameterType.IsAssignableFrom(caster.Entity.GetType()))
                {
                    arguments[i] = caster.Entity;
                }
                else if (parameterType.IsAssignableFrom(target.GetType()))
                {
                    arguments[i] = target;
                }
                else if (parameterType == typeof(bool))
                {
                    arguments[i] = false;
                }
                else if (parameterType == typeof(float))
                {
                    arguments[i] = 999f;
                }
                else if (parameterType == typeof(double))
                {
                    arguments[i] = 999d;
                }
                else if (parameterType.IsValueType)
                {
                    arguments[i] = Activator.CreateInstance(parameterType);
                }
                else
                {
                    arguments[i] = null;
                }
            }

            return true;
        }

        private void SpawnReviveParticles(EntityAgent entity)
        {
            SimpleParticleProperties particles = new SimpleParticleProperties()
            {
                MinPos = entity.Pos.XYZ.AddCopy(-0.8, 0.1, -0.8),
                AddPos = new Vec3d(1.6, 2.0, 1.6),

                MinVelocity = new Vec3f(-0.35f, 0.25f, -0.35f),
                AddVelocity = new Vec3f(0.7f, 1.4f, 0.7f),

                MinSize = 0.3f,
                MaxSize = 1.0f,

                Color = ColorUtil.ToRgba(230, 255, 230, 80),
                LifeLength = 1.1f,
                MinQuantity = 60,
                AddQuantity = 25,
                GravityEffect = -0.03f,
                ParticleModel = EnumParticleModel.Quad
            };

            particles.SizeEvolve = EvolvingNatFloat.create(EnumTransformFunction.LINEAR, -0.45f);
            particles.OpacityEvolve = EvolvingNatFloat.create(EnumTransformFunction.LINEAR, -210f);

            entity.World.SpawnParticles(particles);
        }
    }
}
