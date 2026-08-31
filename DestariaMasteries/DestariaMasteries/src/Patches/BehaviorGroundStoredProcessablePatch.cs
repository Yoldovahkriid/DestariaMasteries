using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Vintagestory.API.Common;

namespace DestariaMasteries.src.Patches
{
    // Transpile patch to modify the drop multiplier for ground-stored processable items based on player stats.
    // Note: This patch is written by AI as I have no idea how to write transpiler patches but it seems to work

    [HarmonyPatch]
    public static class BehaviorGroundStoredProcessablePatch
    {
        public static float GetDropStatMultiplier(BlockDropItemStack drop, IPlayer byPlayer)
        {
            if (drop?.DropModbyStat != null && byPlayer?.Entity != null)
            {
                return byPlayer.Entity.Stats.GetBlended(drop.DropModbyStat);
            }
            return 1f;
        }
        static MethodBase TargetMethod()
        {
            var outerType = AccessTools.TypeByName(
                "Vintagestory.GameContent.CollectibleBehaviorGroundStoredProcessable");

            if (outerType == null)
            {
                throw new Exception(
                    "Patch_GroundStoredProcessable_DropModbyStat: could not find " +
                    "CollectibleBehaviorGroundStoredProcessable - has it been renamed or moved?");
            }

            var getNextItemStack = AccessTools.Method(
                typeof(BlockDropItemStack), nameof(BlockDropItemStack.GetNextItemStack));

            foreach (var nested in outerType.GetNestedTypes(AccessTools.all))
            {
                foreach (var method in nested.GetMethods(AccessTools.all))
                {
                    if (method.IsAbstract || method.ContainsGenericParameters) continue;
                    if (method.GetMethodBody() == null) continue;

                    List<CodeInstruction> instructions;
                    try
                    {
                        instructions = PatchProcessor.GetOriginalInstructions(method).ToList();
                    }
                    catch
                    {
                        continue;
                    }

                    if (instructions.Any(ins => ins.Calls(getNextItemStack)))
                    {
                        return method;
                    }
                }
            }

            throw new Exception(
                "Patch_GroundStoredProcessable_DropModbyStat: could not locate the " +
                "OnContainedInteractStop lambda that calls BlockDropItemStack.GetNextItemStack " +
                "- vssurvivalmod may have changed and this patch needs updating.");
        }

        static IEnumerable<CodeInstruction> Transpiler(
            IEnumerable<CodeInstruction> instructions, MethodBase original)
        {
            var getNextItemStack = AccessTools.Method(
                typeof(BlockDropItemStack), nameof(BlockDropItemStack.GetNextItemStack));
            var helper = AccessTools.Method(
                typeof(BehaviorGroundStoredProcessablePatch), nameof(GetDropStatMultiplier));


            var closureType = original.DeclaringType;
            var byPlayerField = closureType?
                .GetFields(AccessTools.all)
                .FirstOrDefault(f => typeof(IPlayer).IsAssignableFrom(f.FieldType));

            if (byPlayerField == null)
            {
                throw new Exception(
                    "Patch_GroundStoredProcessable_DropModbyStat: could not find the captured " +
                    "'byPlayer' field on the closure type - transpiler needs updating.");
            }

            var code = new List<CodeInstruction>(instructions);

            for (int i = 0; i < code.Count; i++)
            {
                bool isOne =
                    code[i].opcode == OpCodes.Ldc_R4 &&
                    code[i].operand is float f && f == 1f;

                bool nextCallsTarget =
                    i + 1 < code.Count && code[i + 1].Calls(getNextItemStack);

                if (isOne && nextCallsTarget)
                {
                    var replacement = new List<CodeInstruction>
                    {
                        new CodeInstruction(OpCodes.Dup),
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Ldfld, byPlayerField),
                        new CodeInstruction(OpCodes.Call, helper),
                    };

                    code.RemoveAt(i);
                    code.InsertRange(i, replacement);
                    i += replacement.Count - 1;
                }
            }

            return code;
        }
    }
}
