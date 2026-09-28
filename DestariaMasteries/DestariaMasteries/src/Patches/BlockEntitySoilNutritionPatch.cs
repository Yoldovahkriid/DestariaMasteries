using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace DestariaMasteries.src.Patches
{
    [HarmonyPatch(typeof(BlockEntitySoilNutrition), nameof(BlockEntitySoilNutrition.OnBlockInteract))]
    public static class BlockEntitySoilNutritionPatch
    {
        public const string StatName = "saveFertilizerChance";
        public static int RollSeedConsumption(int originalQuantity, IPlayer player)
        {
            if (player?.Entity?.World == null) return originalQuantity;

            float chance = 0f;
            if (player.Entity is EntityAgent byEntity && byEntity.Stats != null)
            {
                chance = byEntity.Stats.GetBlended(StatName) - 1f;
            }
            chance = GameMath.Clamp(chance, 0f, 1f);

            return (player.Entity.World.Rand.NextDouble() < chance) ? 0 : originalQuantity;
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original)
        {
            var codes = new List<CodeInstruction>(instructions);

            MethodInfo takeOutMethod = AccessTools.Method(typeof(ItemSlot), "TakeOut", new[] { typeof(int) });
            if (takeOutMethod == null)
            {
                throw new Exception("Could not find ItemSlot.TakeOut(int) - game API may have changed.");
            }

            MethodInfo helperMethod = AccessTools.Method(
                typeof(BlockEntitySoilNutritionPatch),
                nameof(RollSeedConsumption)
            );

            ParameterInfo[] parameters = original.GetParameters();
            int paramIndex = Array.FindIndex(parameters, p => p.ParameterType == typeof(IPlayer));
            if (paramIndex < 0)
            {
                throw new Exception("Could not find an IPlayer parameter on " +
                    "BlockEntitySoilNutrition.OnBlockInteract - the target method may have changed shape.");
            }
            int argIndex = original.IsStatic ? paramIndex : paramIndex + 1;
            
            CodeInstruction LoadPlayerArg() => new CodeInstruction(OpCodes.Ldarg, argIndex);

            bool patched = false;

            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].Calls(takeOutMethod))
                {
                    codes.Insert(i, new CodeInstruction(OpCodes.Call, helperMethod));
                    codes.Insert(i, LoadPlayerArg());
                    patched = true;
                    break;
                }
            }

            if(!patched)
            {
                throw new Exception("Could not find a call to ItemSlot.TakeOut(int) in " +
                    "BlockEntitySoilNutrition.OnBlockInteract - the target method may have changed shape.");
            }
            return codes;
        }
    }
}
