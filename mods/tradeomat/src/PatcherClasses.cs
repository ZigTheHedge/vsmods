using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace tradeomat.src
{

    [HarmonyPatch(typeof(CollectibleObject), "TryMergeStacks")]
    class PatchTryMergeStacks
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            FieldInfo f_CurrentPriority = AccessTools.Field(typeof(ItemStackMoveOperation), "CurrentPriority");

            bool found = false;
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Ldc_I4_0 && !found)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Ldfld, f_CurrentPriority);
                    found = true;
                }
                else
                    yield return instruction;

            }
        }
    }
}
