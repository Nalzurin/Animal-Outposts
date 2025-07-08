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
            if(pawn.IsNonMutantAnimal && __instance is Outpost_AnimalTraining)
            {
                AnimalOutpostsUtility.SetWantedTrainingAll(pawn);
            }
           
        }
    }
}
