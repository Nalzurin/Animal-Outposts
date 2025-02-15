using AnimalBehaviours;
using Outposts;
using RimWorld.Planet;
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
    public class OffSpringAgeTicks : IExposable
    {
        public int offSpringCount;
        public int ticks;
        public int adultTicks;
        public int juvenileTicks;
        public bool isJuvenile;
        public bool isAdult;
        public bool isChild;
        float adultMinAge;
        float juvenileMinAge;
        public void Tick()
        {
            ticks++;
            if (ticks >= juvenileTicks && !isAdult)
            {
                isJuvenile = true;
                isChild = false;
            }
            if (ticks >= adultTicks)
            {
                isAdult = true;
                isJuvenile = false;
                isChild = false;
            }


        }
        public OffSpringAgeTicks() { }
        public OffSpringAgeTicks(float _adultMinAge, float _juvenileMinAge, int _offSpringCount, float speedModifier = 1f)
        {
            juvenileMinAge = _juvenileMinAge;
            adultMinAge = _adultMinAge;
            offSpringCount = _offSpringCount;
            ticks = 0;
            CalculateTicks(speedModifier);
            //Log.Message(adultTicks);
            isAdult = false;
            isJuvenile = false;
            isChild = true;
        }

        public void CalculateTicks(float speedModifier)
        {
            adultTicks = (int)(adultMinAge * 3600000 * speedModifier);
            juvenileTicks = (int)(juvenileMinAge * 3600000 * speedModifier);
        }
        public void GrowToAdult()
        {
            //Log.Message("Becoming adult");
            //Log.Message("Starting ticks " + ticks);
            //Log.Message("Adult ticks " + adultTicks);
            ticks = adultTicks;
            isAdult = true;
            //Log.Message("New Ticks " + ticks);
        }
        public void ExposeData()
        {
            Scribe_Values.Look(ref offSpringCount, "offSpringCount", 0);
            Scribe_Values.Look(ref ticks, "ticks", 0);
            Scribe_Values.Look(ref adultTicks, "adultTicks", 0);
            Scribe_Values.Look(ref isAdult, "isAdult", false);
            Scribe_Values.Look(ref adultMinAge, "adultMinAge", 0);
            Scribe_Values.Look(ref juvenileTicks, "juvenileTicks", 0);
            Scribe_Values.Look(ref isJuvenile, "isJuvenile", false);
            Scribe_Values.Look(ref juvenileMinAge, "juvenileMinAge", 0);
            Scribe_Values.Look(ref isChild, "isChild", true);
        }
    }
    public class AnimalBreedingPair : IExposable
    {

        public Faction faction;
        public string label;
        public ThingDef animalThingDef;
        public Pawn mAnimal;
        public Pawn fAnimal;
        public List<OffSpringAgeTicks> offSpring;
        public bool isEggLayer;
        public int birthTicks;
        public int currentTicks;
        public float speedModifier;
        public void Tick()
        {
            foreach (OffSpringAgeTicks off in offSpring)
            {
                off.Tick();
            }
            currentTicks--;
            if (currentTicks > 0)
            {
                return;
            }

            currentTicks = birthTicks;
            GiveBirth();

        }
        public void GiveBirth()
        {
            int num = (int)(animalThingDef.HasComp(typeof(CompAsexualReproduction)) ? 1f : (isEggLayer ? ((float)Mathf.Min(animalThingDef.GetCompProperties<CompProperties_EggLayer>().eggCountRange.RandomInRange, animalThingDef.GetCompProperties<CompProperties_EggLayer>().eggFertilizationCountMax)) : ((animalThingDef.race.litterSizeCurve != null) ? Rand.ByCurve(animalThingDef.race.litterSizeCurve) : 1f)));

            offSpring.Add(new OffSpringAgeTicks(animalThingDef.race.lifeStageAges[1].minAge, animalThingDef.race.lifeStageAges[2].minAge, num, speedModifier));
        }
        public void UpdateSpeedModifier(float newSpeedModifier)
        {
            CalculateBirthTicks(newSpeedModifier);
            currentTicks = (int)((currentTicks / speedModifier) * newSpeedModifier);
            foreach (OffSpringAgeTicks offspring in offSpring)
            {
                offspring.CalculateTicks(newSpeedModifier);
            }
            speedModifier = newSpeedModifier;
        }
        public AnimalBreedingPair(Faction _faction, Pawn _fAnimal, Outpost_AnimalBreeding _outpost, float _speedModifier, string _label, Pawn _mAnimal = null)
        {
            label = _label;
            faction = _faction;
            this.animalThingDef = _fAnimal.def;
            this.mAnimal = _mAnimal;
            this.fAnimal = _fAnimal;
            isEggLayer = animalThingDef.HasComp(typeof(CompEggLayer));
            CalculateBirthTicks(_speedModifier);
            currentTicks = birthTicks;
            speedModifier = _speedModifier;
            offSpring = new List<OffSpringAgeTicks>();
        }
        public AnimalBreedingPair() { }
        void CalculateBirthTicks(float modifier)
        {
            birthTicks = (int)((animalThingDef.HasComp(typeof(CompAsexualReproduction)) ? ((float)animalThingDef.GetCompProperties<CompProperties_AsexualReproduction>().reproductionIntervalDays) : (isEggLayer ? animalThingDef.GetCompProperties<CompProperties_EggLayer>().eggLayIntervalDays : animalThingDef.race.gestationPeriodDays)) * 60000f * modifier);
        }

        public IEnumerable<Pawn> generateOffspring(bool doChildren, bool doJuveniles)
        {
            List<OffSpringAgeTicks> offSprings = new List<OffSpringAgeTicks>(offSpring);
            foreach (OffSpringAgeTicks off in offSprings)
            {
                if (off.isChild && !doChildren)
                {
                    continue;
                }
                if (off.isJuvenile && !doJuveniles)
                {
                    continue;
                }
                for (int i = 0; i < off.offSpringCount; i++)
                {
                    Pawn pawn = PawnGenerator.GeneratePawn(fAnimal.kindDef, faction);
                    //Log.Message("Current ticks " + off.ticks);
                    //Log.Message("Adult ticks " + off.adultTicks);
                    pawn.ageTracker.AgeBiologicalTicks = off.ticks;
                    pawn.ageTracker.AgeChronologicalTicks = off.ticks;
                    pawn.relations.AddDirectRelation(PawnRelationDefOf.ParentBirth, fAnimal);
                    if (mAnimal != null)
                    {
                        pawn.relations.AddDirectRelation(PawnRelationDefOf.Parent, mAnimal);
                    }
                    yield return pawn;
                }
                offSpring.Remove(off);
            }
        }
        public void ExposeData()
        {
            Scribe_Deep.Look(ref mAnimal, "mAnimal");
            Scribe_Deep.Look(ref fAnimal, "fAnimal");
            Scribe_Defs.Look(ref animalThingDef, "animalThingDef");
            Scribe_References.Look(ref faction, "faction");
            Scribe_Collections.Look(ref offSpring, "offSpring", LookMode.Deep);
            Scribe_Values.Look(ref isEggLayer, "isEggLayer", false);
            Scribe_Values.Look(ref birthTicks, "birthTicks", 0);
            Scribe_Values.Look(ref currentTicks, "currentTicks", 0);
            Scribe_Values.Look(ref speedModifier, "speedModifier", 0);
            Scribe_Values.Look(ref label, "label", string.Empty);
        }
    }
    public class Outpost_AnimalBreeding : Outpost
    {
        [PostToSetings("AnimalOutposts.Settings.AnimalSkillPerBreedingPair", PostToSetingsAttribute.DrawMode.IntSlider, 10, 1, 20, null, null)]
        public int animalSkillPerBreedingPair = 10;
        [PostToSetings("AnimalOutposts.Settings.BirthingGrowingTimeModifier", PostToSetingsAttribute.DrawMode.Slider, 1f, 0.1f, 10f, null, null)]
        public float birthingGrowingTimeModifier = 1f;

        private float previousModifier = 1f;

        private bool deliverJuveniles = false;

        List<AnimalBreedingPair> animalBreedingPairs = [];

        Dictionary<ThingDef, int> animalDefIndexes = [];

        int maxPairs => this.TotalSkill(Ext.RequiredSkills.First().Skill) / animalSkillPerBreedingPair;

        public override void Tick()
        {
            base.Tick();
            if (Packing && !animalBreedingPairs.Empty())
            {
                List<AnimalBreedingPair> local = new List<AnimalBreedingPair>(animalBreedingPairs);
                foreach (AnimalBreedingPair pair in local)
                {
                    RemovePair(pair);
                }
            }
            if (previousModifier != birthingGrowingTimeModifier)
            {
                UpdateMultiplier();
            }
            foreach (AnimalBreedingPair pair in animalBreedingPairs)
            {
                pair.Tick();
            }
        }


        public override string ProductionString()
        {
            StringBuilder sb = new StringBuilder("AnimalOutposts.ProductionString.PairLimit".Translate(animalBreedingPairs.Count, maxPairs));
            if (!animalBreedingPairs.Empty())
            {
                sb.Append("\n" + "AnimalOutposts.ProductionString.BreedingPairs".Translate());
                foreach (AnimalBreedingPair pair in animalBreedingPairs)
                {
                    sb.Append("\n- " + pair.label);
                    int adults = 0;
                    int children = 0;
                    int juveniles = 0;
                    int newBirthTicks = pair.currentTicks;
                    int newAdultTicks = int.MaxValue;
                    foreach (OffSpringAgeTicks off in pair.offSpring)
                    {
                        if (off.adultTicks - off.ticks < newAdultTicks)
                        {
                            newAdultTicks = off.adultTicks - off.ticks;
                        }
                        if (off.isAdult)
                        {
                            adults += off.offSpringCount;
                        }
                        else if (off.isJuvenile)
                        {
                            juveniles += off.offSpringCount;
                        }
                        else
                        {
                            children += off.offSpringCount;
                        }
                    }
                    sb.Append("\n  " + "AnimalOutposts.ProductionString.Adults".Translate(adults));
                    sb.Append("\n  " + "AnimalOutposts.ProductionString.Juveniles".Translate(juveniles));
                    sb.Append("\n  " + "AnimalOutposts.ProductionString.Children".Translate(children));
                    sb.Append("\n  " + "AnimalOutposts.ProductionString.NewBirthIn".Translate(newBirthTicks.ToStringTicksToPeriodVerbose()));
                    if (pair.offSpring.Where(c => !c.isAdult).Any())
                    {
                        sb.Append("\n  " + "AnimalOutposts.ProductionString.NewAdultIn".Translate(newAdultTicks.ToStringTicksToPeriodVerbose()));

                    }

                }

            }
            else
            {
                sb.Append("\n" + "AnimalOutposts.ProductionString.NoPairsNotProducing".Translate());
            }
            return sb.ToString();
        }
        public override void Produce()
        {
            if (ShouldProduce())
            {
                Deliver(ProducedThings());
            }
        }
        public bool ShouldProduce()
        {
            foreach (AnimalBreedingPair pair in animalBreedingPairs)
            {
                if (pair.offSpring.Any(c => c.isAdult || (deliverJuveniles && c.isJuvenile)))
                {
                    return true;
                }
            }
            return false;
        }
        public override IEnumerable<Thing> ProducedThings()
        {
            foreach (AnimalBreedingPair pair in animalBreedingPairs)
            {
                foreach (Pawn pawn in pair.generateOffspring(false, deliverJuveniles))
                {
                    yield return pawn;
                }
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }
            Command_Action addPair = new Command_Action
            {
                action = delegate
                {
                    Find.WindowStack.Add(new FloatMenu(GetAllAnimalPairsOptions(AllPawns.ToList()).ToList()));
                },
                defaultLabel = "AnimalOutposts.Commands.AddPair.Label".Translate(),
                defaultDesc = "AnimalOutposts.Commands.AddPair.Desc".Translate(),
                icon = Textures.AddPair
            };
            if (animalBreedingPairs.Count >= maxPairs)
            {
                addPair.Disabled = true;
                addPair.disabledReason = "AnimalOutposts.Command.AddPair.MaxPairsReached".Translate();
            }
            if (!AnyAnimalPairs(AllPawns.ToList()))
            {
                addPair.Disabled = true;
                addPair.disabledReason = "AnimalOutposts.Command.AddPair.NoPairs".Translate();
            }
            if (Packing)
            {
                addPair.Disabled = true;
                addPair.disabledReason = "AnimalOutposts.Command.AddPair.Packing".Translate();
            }
            yield return addPair;

            Command_Action removePair = new Command_Action
            {
                action = delegate
                {
                    Find.WindowStack.Add(new FloatMenu(animalBreedingPairs.Select(c => new FloatMenuOption(c.label, delegate
                    {
                        RemovePair(c);
                    })).ToList()));
                },
                defaultLabel = "AnimalOutposts.Commands.RemovePair.Label".Translate(),
                defaultDesc = "AnimalOutposts.Commands.RemovePair.Desc".Translate(),
                icon = Textures.RemovePair
            };
            if (animalBreedingPairs.Count == 0)
            {
                removePair.Disabled = true;
                removePair.disabledReason = "AnimalOutposts.Command.AddPair.NoActivePairs".Translate();
            }
            yield return removePair;


            Command_Toggle shouldDeliverJuveniles = new Command_Toggle
            {
                isActive = () => deliverJuveniles,
                toggleAction = delegate
                {
                    deliverJuveniles = !deliverJuveniles;
                },
                defaultLabel = "AnimalOutposts.Commands.DeliverJuveniles.Label".Translate(),
                defaultDesc = "AnimalOutposts.Commands.DeliverJuveniles.Desc".Translate(),
                icon = Textures.DeliverJuveniles
            };
            yield return shouldDeliverJuveniles;

            Command_Action deliverEarly = new Command_Action
            {
                action = delegate
                {
                    List<Pawn> offspring = [];
                    foreach (AnimalBreedingPair pair in animalBreedingPairs)
                    {
                        foreach (Pawn pawn in pair.generateOffspring(true, true))
                        {
                            offspring.Add(pawn);
                        }
                    }
                    Deliver(offspring);
                },
                defaultLabel = "AnimalOutposts.Commands.DeliverEarly.Label".Translate(),
                defaultDesc = "AnimalOutposts.Commands.DeliverEarly.Desc".Translate(),
                icon = Textures.DeliverEarly
            };
            if (animalBreedingPairs.Count == 0)
            {
                removePair.Disabled = true;
                removePair.disabledReason = "AnimalOutposts.Command.AddPair.NoActivePairs".Translate();
            }
            yield return deliverEarly;


            if (Prefs.DevMode)
            {
                yield return new Command_Action
                {
                    action = delegate
                    {
                        foreach (AnimalBreedingPair pair in animalBreedingPairs)
                        {
                            pair.currentTicks = 10;
                        }
                    },
                    defaultLabel = "Dev: Birth now",
                    defaultDesc = "Reduce ticksTillBirth to 10"
                };
                yield return new Command_Action
                {
                    action = delegate
                    {
                        foreach (AnimalBreedingPair pair in animalBreedingPairs)
                        {
                            foreach (OffSpringAgeTicks tick in pair.offSpring)
                            {
                                tick.GrowToAdult();
                            }
                        }
                    },
                    defaultLabel = "Dev: Grow to adult",
                    defaultDesc = "Grow all offspring to adult age"
                };
            }
        }

        public void RemovePair(AnimalBreedingPair pair)
        {
            if (pair.mAnimal != null)
            {
                AddPawn(pair.mAnimal);
            }
            AddPawn(pair.fAnimal);
            foreach (Pawn p in pair.generateOffspring(true, true))
            {
                AddPawn(p);
            }
            animalBreedingPairs.Remove(pair);
        }

        public void AddPair(Pawn female, Pawn male = null)
        {
            if (animalDefIndexes.ContainsKey(female.def))
            {
                animalDefIndexes[female.def] += 1;
            }
            else
            {
                animalDefIndexes[female.def] = 1;

            }
            animalBreedingPairs.Add(new AnimalBreedingPair(Faction, female, this, birthingGrowingTimeModifier, $"{female.GetKindLabelPlural(2)} {animalDefIndexes[female.def]}", male));
            if (male != null)
            {
                this.RemovePawn(male);
            }
            this.RemovePawn(female);

        }
        public void UpdateMultiplier()
        {
            foreach (AnimalBreedingPair pair in animalBreedingPairs)
            {
                pair.UpdateSpeedModifier(birthingGrowingTimeModifier);
            }
            previousModifier = birthingGrowingTimeModifier;
        }
        public IEnumerable<FloatMenuOption> GetAllAnimalPairsOptions(List<Pawn> pawns)
        {
            List<Pawn> list = pawns.Where((Pawn p) => p.IsNonMutantAnimal).ToList();
            foreach (ThingDef raceType in list.Select((Pawn p) => p.def).Distinct().ToList())
            {
                if (raceType.race.gestationPeriodDays > 0 && (raceType.race.hasGenders && list.Any((Pawn p) => p.def == raceType && p.gender == Gender.Female && p.ageTracker.CurLifeStageIndex == p.ageTracker.MaxRaceLifeStageIndex) && list.Any((Pawn p) => p.def == raceType && p.gender == Gender.Male && p.ageTracker.CurLifeStageIndex == p.ageTracker.MaxRaceLifeStageIndex)))
                {
                    yield return new FloatMenuOption(raceType.LabelCap, delegate
                    {
                        AddPair(list.Where(p => p.def == raceType && p.gender == Gender.Female && p.ageTracker.CurLifeStageIndex == p.ageTracker.MaxRaceLifeStageIndex).RandomElement(), list.Where(p => p.def == raceType && p.gender == Gender.Male && p.ageTracker.CurLifeStageIndex == p.ageTracker.MaxRaceLifeStageIndex).RandomElement());
                    });
                }
                if ((!raceType.race.hasGenders || raceType.HasComp(typeof(CompAsexualReproduction))) && list.Any((Pawn p) => p.def == raceType))
                {
                    yield return new FloatMenuOption(raceType.LabelCap, delegate
                    {
                        AddPair(list.Where(p => p.def == raceType && p.ageTracker.CurLifeStageIndex == p.ageTracker.MaxRaceLifeStageIndex).RandomElement());
                    });
                }
            }
        }
        public static bool AnyAnimalPairs(List<Pawn> pawns)
        {
            List<Pawn> list = pawns.Where((Pawn p) => p.IsNonMutantAnimal).ToList();
            foreach (ThingDef raceType in list.Select((Pawn p) => p.def).Distinct().ToList())
            {
                if (raceType.race.gestationPeriodDays > 0 && (raceType.race.hasGenders && list.Any((Pawn p) => p.def == raceType && p.gender == Gender.Female && p.ageTracker.CurLifeStageIndex == p.ageTracker.MaxRaceLifeStageIndex) && list.Any((Pawn p) => p.def == raceType && p.gender == Gender.Male && p.ageTracker.CurLifeStageIndex == p.ageTracker.MaxRaceLifeStageIndex)) || ((!raceType.race.hasGenders || raceType.HasComp(typeof(CompAsexualReproduction))) && list.Any((Pawn p) => p.def == raceType && p.ageTracker.CurLifeStageIndex == p.ageTracker.MaxRaceLifeStageIndex)))
                {
                    return true;
                }
            }
            return false;
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref deliverJuveniles, "deliverJuveniles", false);
            Scribe_Values.Look(ref previousModifier, "previousModifier", 1f);
            Scribe_Collections.Look(ref animalBreedingPairs, "animalBreedingPairs", LookMode.Deep);
            Scribe_Collections.Look(ref animalDefIndexes, "animalDefIndexes", LookMode.Def, LookMode.Value);

        }


    }
}
