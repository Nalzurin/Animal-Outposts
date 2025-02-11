using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AnimalOutposts
{
    public static class AnimalOutpostsUtility
    {
        public static void SetWantedTrainingAll(Pawn animal)
        {
            //Log.Message(animal.Label);

            List<TrainableDef> trainableDefsInListOrder = TrainableUtility.TrainableDefsInListOrder;
            foreach (TrainableDef d in trainableDefsInListOrder)
            {
                if (animal.training.CanAssignToTrain(d))
                {
                    animal.training.SetWantedRecursive(d, true);
                }
            }
        }
    }
}
