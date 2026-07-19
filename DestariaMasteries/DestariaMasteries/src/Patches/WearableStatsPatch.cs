using HarmonyLib;
using MasteryLibrary.src.Behaviors.EntityBehaviors;
using MasteryLibrary.src.Core.Masteries.Instances;
using MasteryLibrary.src.Utilities;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace DestariaMasteries.src.Patches
{
    [HarmonyPatch]
    public class WearableStatsPatch
    {
        static System.Reflection.MethodBase TargetMethod()
        {
            return AccessTools.Method(typeof(ModSystemWearableStats), "updateWearableStats");
        }

        public static bool Prefix(ModSystemWearableStats __instance, IInventory inv, IServerPlayer player)
        {
            PlayerMasteryData? playerdata = player.Entity.GetBehavior<EntityBehaviorPlayerMasteries>()?.PlayerMasteryData;
            if (playerdata == null) return true;

            foreach (var slot in inv)
            {
                if (slot.Empty || slot.Itemstack.Collectible.GetCollectibleInterface<IWearableStatsSupplier>() is not IWearableStatsSupplier) continue;
                bool isGlasses = slot.Itemstack.Collectible.Attributes?["isGlasses"].AsBool() ?? false;
                if (!isGlasses) continue;

                if (playerdata.HasSkill("EyeForCraftsmanship"))
                {
                    SkillInstance sInst = playerdata.GetSkillInstance("EyeForCraftsmanship");
                    if (sInst == null) continue;
                    var glassesBonus = sInst.Skill.GetValueSafe<Dictionary<string, StatConfiguration>>("glassesbonus");
                    foreach (var kvp in glassesBonus)
                    {
                        player.Entity.Stats.Set(kvp.Key, "glassesWearableMod", StatScalingUtil.GetScaledValue(kvp.Value, sInst.Level));
                    }
                }
            }
            return true;
        }
    }
}
