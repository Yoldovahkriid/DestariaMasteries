using HarmonyLib;
using MasteryLibrary.src.Behaviors.EntityBehaviors;
using MasteryLibrary.src.Core.Masteries.Instances;
using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace DestariaMasteries.src.Patches
{
    [HarmonyPatch(typeof(BlockEntityQuern), nameof(BlockEntityQuern.GrindSpeed), MethodType.Getter)]
    public static class BlockEntityQuernPatch
    {
        static void Postfix(BlockEntityQuern __instance, ref float __result)
        {
            if (__result <= 0f) return;

            var field = AccessTools.Field(typeof(BlockEntityQuern), "playersGrinding");
            if (field?.GetValue(__instance) is not Dictionary<string, long> playersGrinding
                || playersGrinding.Count == 0)
            {
                return;
            }

            ICoreAPI api = __instance.Api;
            if (api?.World == null) return;

            foreach (string uid in playersGrinding.Keys)
            {
                IPlayer player = api.World.PlayerByUid(uid);
                EntityAgent? byEntity = player?.Entity;
                if (byEntity == null) continue;

                PlayerMasteryData? data = byEntity.GetBehavior<EntityBehaviorPlayerMasteries>()?.PlayerMasteryData;
                bool hasSkill = data != null && data.HasSkill("TheMiller");
                SkillInstance? skill = data?.GetSkillInstance("TheMiller");
                if (skill?.Level != 5) continue; // only level 5 of the skill grants the bonus

                if (hasSkill)
                {
                    __result *= 4f;
                    break;
                }
            }
        }
    }
}
