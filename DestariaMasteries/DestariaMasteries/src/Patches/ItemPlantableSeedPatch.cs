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
    [HarmonyPatch(typeof(ItemPlantableSeed), nameof(ItemPlantableSeed.OnHeldInteractStart))]

    public static class ItemPlantableSeedPatch

    {
        public const string StatName = "saveSeedChance";
        public const string FreeSeedAttr = "masteryFreeSeedPlant";
        public static int RollSeedConsumption(int originalQuantity, EntityAgent byEntity, BlockSelection blockSel)
        {
            if (byEntity?.World == null) return originalQuantity;
            float chance = 0f;

            if (byEntity.Stats != null)
            {
                chance = byEntity.Stats.GetBlended(StatName) - 1f;
            }

            chance = GameMath.Clamp(chance, 0f, 1f);

            bool seedSaved = byEntity.World.Rand.NextDouble() < chance;

            if (seedSaved && blockSel != null)
            {
                if (byEntity.World.BlockAccessor.GetBlockEntity(blockSel.Position) is BlockEntityFarmland beFarmland)
                {
                    beFarmland.CropAttributes.SetBool(FreeSeedAttr, true);
                    beFarmland.MarkDirty(true);
                }
            }

            return seedSaved ? 0 : originalQuantity;
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
                typeof(ItemPlantableSeedPatch),
                nameof(RollSeedConsumption)
            );

            ParameterInfo[] parameters = original.GetParameters();

            int entityParamIndex = Array.FindIndex(parameters, p => p.ParameterType == typeof(EntityAgent));

            if (entityParamIndex < 0)
            {
                throw new Exception("Could not find an EntityAgent parameter on " +
                    "ItemPlantableSeed.OnHeldInteractStart - the target method may have changed shape.");
            }

            int blockSelParamIndex = Array.FindIndex(parameters, p => p.ParameterType == typeof(BlockSelection));

            if (blockSelParamIndex < 0)
            {
                throw new Exception("Could not find a BlockSelection parameter on " +
                    "ItemPlantableSeed.OnHeldInteractStart - the target method may have changed shape.");
            }

            int entityArgIndex = original.IsStatic ? entityParamIndex : entityParamIndex + 1;
            int blockSelArgIndex = original.IsStatic ? blockSelParamIndex : blockSelParamIndex + 1;

            CodeInstruction LoadArg(int argIndex)
            {
                switch (argIndex)
                {
                    case 0: return new CodeInstruction(OpCodes.Ldarg_0);
                    case 1: return new CodeInstruction(OpCodes.Ldarg_1);
                    case 2: return new CodeInstruction(OpCodes.Ldarg_2);
                    case 3: return new CodeInstruction(OpCodes.Ldarg_3);
                    default:
                        return argIndex <= 255
                            ? new CodeInstruction(OpCodes.Ldarg_S, (byte)argIndex)
                            : new CodeInstruction(OpCodes.Ldarg, argIndex);
                }
            }

            bool patched = false;

            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].Calls(takeOutMethod))
                {
                    codes.Insert(i, new CodeInstruction(OpCodes.Call, helperMethod));
                    codes.Insert(i, LoadArg(blockSelArgIndex));
                    codes.Insert(i, LoadArg(entityArgIndex));
                    patched = true;
                    break;
                }
            }

            if (!patched)
            {
                throw new Exception("Failed to locate ItemSlot.TakeOut(1) call in " +
                    "ItemPlantableSeed.OnHeldInteractStart - the target method may have changed shape " +
                    "in a game update. Patch needs updating.");
            }

            return codes;
        }
    }
}
