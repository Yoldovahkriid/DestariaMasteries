using HarmonyLib;
using MasteryLibrary.src.Behaviors.EntityBehaviors;
using System;
using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;
using static OpenTK.Graphics.OpenGL.GL;

#nullable disable

namespace YourModNamespace
{
    [HarmonyPatch]
    public static class ItemHackingSpearPatch
    {
        static System.Reflection.MethodBase TargetMethod()
        {
            return AccessTools.Method(typeof(ItemHackingSpear), "ItemSpear_OnBeginHitEntity");
        }

        [HarmonyPrefix]
        public static bool Prefix(ItemHackingSpear __instance, EntityAgent byEntity, ref EnumHandling handling)
        {
            if (byEntity.World.Side == EnumAppSide.Client)
            {
                return true;
            }

            var eplr = byEntity as EntityPlayer;
            var entitySel = (eplr)?.EntitySelection;

            if (byEntity.Attributes.GetInt("didattack") == 0)
            {
                byEntity.Attributes.SetInt("didattack", 1);

                var slot = byEntity.ActiveHandItemSlot;

                int toolMode = __instance.GetToolMode(byEntity.RightHandItemSlot, eplr.Player, eplr.BlockSelection);

                if (toolMode == 1)
                {
                    return true;
                }

                if (entitySel == null) return true;

                if (byEntity.GetBehavior<EntityBehaviorPlayerMasteries>() == null) return true;

                bool canhackEntity =
                    entitySel.Entity.Properties.Attributes?["hackedEntity"].Exists == true
                    && byEntity.GetBehavior<EntityBehaviorPlayerMasteries>().PlayerMasteryData.HasSkill("Tuning");

                ICoreServerAPI sapi = byEntity.World.Api as ICoreServerAPI;

                if (canhackEntity)
                {
                    sapi.World.PlaySoundAt(new AssetLocation("sounds/player/hackingspearhit.ogg"), entitySel.Entity, null);

                    if (sapi.World.Rand.NextDouble() < 0.15)
                    {
                        InvokeSpawnEntityInPlaceOf(__instance, entitySel.Entity, entitySel.Entity.Properties.Attributes["hackedEntity"].AsString(), byEntity);
                        sapi.World.DespawnEntity(entitySel.Entity, new EntityDespawnData() { Reason = EnumDespawnReason.Removed });
                        return false;
                    }
                }
            }

            return true;
        }

        static void InvokeSpawnEntityInPlaceOf(object instance, Entity byEntity, string code, EntityAgent causingEntity)
        {
            var method = AccessTools.Method(typeof(ItemHackingSpear), "SpawnEntityInPlaceOf");
            method.Invoke(instance, new object[] { byEntity, code, causingEntity });
        }
    }
}