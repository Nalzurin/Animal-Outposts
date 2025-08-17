using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Outposts;
using RimWorld;
using VEF.Weapons;

namespace AnimalOutposts
{
    [StaticConstructorOnStartup]
    public static class HarmonyStarter
    {
        static HarmonyStarter()
        {
            Harmony harmony = new Harmony("AnimalOutposts");
            harmony.PatchAll();
        }
    }
    [HarmonyPatch]
    public static class OutpostAddPawnAnimalPatch
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.Method(typeof(Outpost), nameof(Outpost.AddPawn));
        }
        private static void Postfix(Pawn pawn, Outpost __instance)
        {
            //Log.Message(pawn.Label);
            //Log.Message(__instance is Outpost_AnimalTraining);
            if (pawn.IsAnimal && !pawn.IsMutant && __instance is Outpost_AnimalTraining)
            {
                AnimalOutpostsUtility.SetWantedTrainingAll(pawn);
            }

        }
        private static bool Comparer(Verb verb)
        {
            return verb is Verb;
        }
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            bool FoundIsInst = false;
            bool DoPatch = false;
            bool blockFlag = false;
            object operand = null;
            foreach (CodeInstruction instruction in instructions)
            {

                if (DoPatch)
                {
                    DoPatch = false;
                    yield return new(OpCodes.Ldarg_1);
                    yield return new(OpCodes.Call, AccessTools.Method(typeof(OutpostAddPawnAnimalPatch), "Comparer"));
                    CodeInstruction codeInstruction = new(OpCodes.Brtrue_S, operand);
                }
                if (FoundIsInst)
                {
                    if (instruction.opcode == OpCodes.Brtrue_S)
                    {
                        DoPatch = true;
                        FoundIsInst = false;
                        operand = instruction.operand;

                    }
                }
                if (instruction.opcode == OpCodes.Isinst && instruction.operand.GetType() == typeof(Verb_MeleeAttack))
                {
                    FoundIsInst = true;
                    blockFlag = true;
                }
                yield return instruction;
            }
        }
    }
}
