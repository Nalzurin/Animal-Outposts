using Outposts;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace AnimalOutposts
{
    public class Outpost_AnimalTraining : Outpost
    {

        [PostToSetings("Outposts.Settings.TrainingInterval", PostToSetingsAttribute.DrawMode.Time, 60000, 2500f, 420000f, null, null)]
        public int intervalBetweenTraining = 60000;
        [PostToSetings("Outposts.Settings.SkillAmountPerAnimal", PostToSetingsAttribute.DrawMode.IntSlider, 8, 1, 20, null, null)]
        public int skillAmountPerAnimal = 8;

/*        [PostToSetings("Outposts.Settings.TrainingRandom", PostToSetingsAttribute.DrawMode.Checkbox, true, 0f, 0f, null, null)]
        public bool trainingRandom = true;*/
        private int ticksToNextTrain;

        public override void PostMake()
        {
            base.PostMake();
            ticksToNextTrain = intervalBetweenTraining;
            foreach (Pawn p in GetAnimals())
            {
                AnimalOutpostsUtility.SetWantedTrainingAll(p);

            }
        }

        public override void PostAdd()
        {
            //Log.Message("test");
            base.PostAdd();

        }
        public override void Tick()
        {
            base.Tick();
            ticksToNextTrain--;
            if (ticksToNextTrain >= 0)
            {
                return;
            }
            ticksToNextTrain = intervalBetweenTraining;
            if (!GetAnimals().Any())
            {
                return;
            }
            if (base.Packing)
            {
                return;
            }
            int count = base.TotalSkill(Ext.RequiredSkills[0].Skill) / Ext.RequiredSkills[0].Count;
            List<Pawn> animals = GetAnimals().Except(GetTrainedAnimals()).ToList();
            for (int i = 0; i < count; i++)
            {
                if (animals.Count() == 0)
                {
                    //Log.Message("Animals empty");
                    break;
                }
                Pawn animal;
/*                if (trainingRandom)
                {*/

                    int rand = Rand.Range(0, animals.Count());
                    animal = animals[rand];
                    animals.RemoveAt(rand);
/*                }
                else
                {
                    animal = animals[0];
                    animals.RemoveAt(0);
                }*/

                TrainableDef def = animal.training.NextTrainableToTrain();
                if (def == null)
                {
                    i--;
                    continue;
                }
                //Log.Message($"Training {animal.Label}'s {def.label}");
                animal.training.Train(def, null);

            }
        }
        public override string ProductionString()
        {
            if (ProducedThings().Count() == 0)
            {
                return "AnimalOutposts.NoTrainedAnimalsToDeliver".Translate();
            }
            else
            {
                StringBuilder sb = new StringBuilder("AnimalOutposts.WillDeliverTrainedAnimals".Translate(TimeTillProduction));
                foreach (Pawn p in ProducedThings())
                {
                    sb.Append($"\n- {p.LabelCap}");
                }
                return sb.ToString();
            }


        }
        public override void Produce()
        {
            if (ProducedThings().Count() > 0)
            {
                
                List<Thing> things = new List<Thing>(ProducedThings());
                foreach (Pawn t in things)
                {
                    RemovePawn(t);
                }
                Deliver(things);
            }
        }
        public override IEnumerable<Thing> ProducedThings()
        {
           /* foreach (var item in GetTrainedAnimals())
            {
                Log.Message(item.Label);
            }*/
            return GetTrainedAnimals();
        }
        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }
            if (Prefs.DevMode)
            {
                yield return new Command_Action
                {
                    action = delegate
                    {
                        ticksToNextTrain = 5;
                    },
                    defaultLabel = "Dev: Train now",
                    defaultDesc = "Train animals now"
                };
            }
        }
        /*        public TrainableDef NextTrainableToTrain(Pawn animal)
                {
                    List<TrainableDef> trainableDefsInListOrder = TrainableUtility.TrainableDefsInListOrder;
                    for (int i = 0; i < trainableDefsInListOrder.Count; i++)
                    {
                        if (animal.training.CanBeTrained(trainableDefsInListOrder[i]))
                        {
                            return trainableDefsInListOrder[i];
                        }
                    }
                    return null;
                }*/
        public IEnumerable<Pawn> GetAnimals()
        {
            return base.AllPawns.Where(c => c.IsAnimal && !c.IsMutant);
        }
        public IEnumerable<Pawn> GetTrainedAnimals()
        {

            return GetAnimals().Where(c => c.training.NextTrainableToTrain() == null);
        }

        /*        public override string GetInspectString()
                {
                    StringBuilder stringBuilder = new StringBuilder(base.GetInspectString());
                    stringBuilder.AppendInNewLine("YHMisc_OutpostTraining.CurSkill".Translate(curSkill.LabelCap));
                    stringBuilder.AppendInNewLine("YHMisc_OutpostTraining.CurMaxXPperDay".Translate(maxXPperDay));
                    if (useMaxSkillLevel)
                    {
                        stringBuilder.AppendInNewLine("YHMisc_OutpostTraining.CurMaxLevel".Translate(maxSkillLevel));
                    }
                    return stringBuilder.ToString();
                }*/

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ticksToNextTrain, "ticksToNextTrain", 0);
        }
    }
}
