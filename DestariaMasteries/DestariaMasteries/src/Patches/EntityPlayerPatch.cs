using HarmonyLib;
using MasteryLibrary.src.Behaviors.EntityBehaviors;
using MasteryLibrary.src.Core.Masteries.Instances;
using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace DestariaMasteries.src.Patches
{
    [HarmonyPatch(typeof(Entity), "ReceiveDamage")]
    public class EntityPatch
    {
        private const string TurnBackTheClockLastUseDayKey = "destariamasteries:turnbacktheclock-lastusedayutc";

        private static long GetUtcDayIndex()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds() / 86400L;
        }

        private static bool TurnBackTheClockUsedToday(EntityPlayer player)
        {
            long lastUseDay = player.WatchedAttributes.GetLong(TurnBackTheClockLastUseDayKey, -1L);
            return lastUseDay == GetUtcDayIndex();
        }

        private static void MarkTurnBackTheClockUsed(EntityPlayer player)
        {
            player.WatchedAttributes.SetLong(TurnBackTheClockLastUseDayKey, GetUtcDayIndex());
        }

        public static bool Prefix(Entity __instance, DamageSource damageSource, float damage)
        {
            if (__instance is EntityPlayer player && player.HasBehavior<EntityBehaviorHealth>())
            {
                EntityBehaviorHealth? ebh = player.GetBehavior<EntityBehaviorHealth>();
                if (ebh != null && ebh.Health - damage <= 0.0f)
                {
                    EntityBehaviorPlayerMasteries? ebpm = player.GetBehavior<EntityBehaviorPlayerMasteries>();
                    if (ebpm == null) return true;
                    bool hasskill = ebpm.PlayerMasteryData.HasSkill("TurnBackTheClock");
                    if (hasskill && !TurnBackTheClockUsedToday(player))
                    {
                        SkillInstance? sinst = ebpm.PlayerMasteryData.GetSkillInstance("TurnBackTheClock");
                        float healthRefund = player.Stats.GetBlended("deathHealthRefund") - 1.0f;
                        ebh.Health = ebh.MaxHealth * healthRefund;

                        if (player.Player is IServerPlayer serverPlayer)
                        {
                            serverPlayer.SendMessage(GlobalConstants.AllChatGroups, "Time fractures around you...", EnumChatType.Notification);
                            EntityPos spawnPos = serverPlayer.GetSpawnPosition(false);
                            player.TeleportTo(spawnPos);
                        }

                        MarkTurnBackTheClockUsed(player);

                        // Skip the original ReceiveDamage call so the killing blow never lands
                        return false;
                    }
                }
            }

            return true;
        }
    }
}