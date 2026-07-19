using Crossbows;
using HarmonyLib;
using System;
using OpenTK.Mathematics;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Server;
using CombatOverhaul.RangedSystems;
using CombatOverhaul.Implementations;
using Vintagestory.API.MathTools;

namespace DestariaMasteries.src.Patches
{
    [HarmonyPatch(typeof(CrossbowServer), nameof(CrossbowServer.Shoot))]
    public static class CrossbowDamagePatch
    {
        public const string DamageStatName = "crossbowDamageMultiplier";

        [HarmonyPostfix]
        public static void Postfix() { }

        [HarmonyPrefix]
        public static bool Prefix(
            CrossbowServer __instance,
            IServerPlayer player,
            ItemSlot slot,
            ShotPacket packet,
            Entity shooter,
            ref bool __result,
            Dictionary<long, (InventoryBase, int)> ____boltSlots,
            ProjectileSystemServer ____projectileSystem,
            CrossbowStats ____stats)
        {
            if (!____boltSlots.ContainsKey(player.Entity.EntityId))
            {
                __result = false;
                return false;
            }

            (InventoryBase inventory, int slotId) = ____boltSlots[player.Entity.EntityId];

            if (inventory.Count <= slotId)
            {
                __result = false;
                return false;
            }

            ItemSlot? boltSlot = inventory[slotId];

            if (boltSlot?.Itemstack == null || boltSlot.Itemstack.StackSize < 1)
            {
                __result = false;
                return false;
            }

            ProjectileStats? stats = boltSlot.Itemstack.Item
                .GetCollectibleBehavior<ProjectileBehavior>(true)
                ?.GetStats(boltSlot.Itemstack);

            if (stats == null)
            {
                ____boltSlots.Remove(player.Entity.EntityId);
                __result = false;
                return false;
            }

            ItemStackRangedStats stackStats = ItemStackRangedStats.FromItemStack(slot.Itemstack);

            float statMultiplier = GetDamageStat(player.Entity);
            float crossbowdispersionmult = player.Entity.Stats.GetBlended("crossbowDispersion");

            Vector3d playerVelocity = new(
                player.Entity.Pos.Motion.X,
                player.Entity.Pos.Motion.Y,
                player.Entity.Pos.Motion.Z);

            Vector3d projectileDirection = Traverse.Create(__instance)
                .Method("GetDirectionWithDispersion",
                    packet.Velocity,
                    new float[]
                    {
                    ____stats.DispersionMOA[0] * stackStats.DispersionMultiplier * crossbowdispersionmult,
                    ____stats.DispersionMOA[1] * stackStats.DispersionMultiplier * crossbowdispersionmult
                    })
                .GetValue<Vector3d>();

            Vector3d projectileVelocity =
                projectileDirection * ____stats.BoltVelocity * stackStats.ProjectileSpeed + playerVelocity;

            ProjectileSpawnStats spawnStats = new()
            {
                ProducerEntityId = player.Entity.EntityId,
                DamageMultiplier = ____stats.BoltDamageMultiplier * stackStats.DamageMultiplier * statMultiplier,
                DamageTier = (int)____stats.BoltDamageStrength + stackStats.DamageTierBonus,
                Position = new Vector3d(packet.Position[0], packet.Position[1], packet.Position[2]),
                Velocity = projectileVelocity,
            };

            ____projectileSystem.SpawnFromWeaponSlot(
                packet.ProjectileId[0], stats, spawnStats, boltSlot.TakeOut(1), slot, shooter);

            boltSlot.MarkDirty();

            slot.Itemstack.Item.DamageItem(
                player.Entity.World, player.Entity, slot, 1 + stats.AdditionalDurabilityCost);
            slot.Itemstack.Attributes.SetBool("crossbow-drawn", false);
            slot.MarkDirty();

            __result = true;
            return false;
        }

        private static float GetDamageStat(EntityPlayer entity)
        {
            float raw = entity.Stats.GetBlended(DamageStatName);
            return raw <= 0f ? 1f : raw;
        }
    }

}
