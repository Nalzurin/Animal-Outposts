using Outposts;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace AnimalOutposts
{
    public class WITab_Outpost_AnimalTraining : WITab
    {


        private Vector2 scrollPosition;

        private float scrollViewHeight;

        private Pawn specificTrainingTabForpawn;


        public Outpost_AnimalTraining SelOutpost => base.SelObject as Outpost_AnimalTraining;


        protected List<Pawn> Pawns => SelOutpost.GetAnimals().ToList();

        public WITab_Outpost_AnimalTraining()
        {
            labelKey = "TabTraining";
            tutorTag = "Training";
        }

        public override void Notify_ClearingAllMapsMemory()
        {
            base.Notify_ClearingAllMapsMemory();
            specificTrainingTabForpawn = null;
        }

        protected override void UpdateSize()
        {
            EnsureSpecificNeedsTabForPawnValid();
            base.UpdateSize();
            size = CaravanNeedsTabUtility.GetSize(Pawns, PaneTopY, false);
        }

        protected override void ExtraOnGUI()
        {
            EnsureSpecificNeedsTabForPawnValid();
            base.ExtraOnGUI();
            Pawn localSpecificTrainingTabForpawn = specificTrainingTabForpawn;
            if (localSpecificTrainingTabForpawn == null)
            {
                return;
            }
            Rect tabRect = base.TabRect;
            Rect rect = new Rect(tabRect.xMax - 1f, tabRect.yMin, 300f, 200f);
            Find.WindowStack.ImmediateWindow(1439870015, rect, WindowLayer.GameUI, delegate
            {
                if (!localSpecificTrainingTabForpawn.DestroyedOrNull())
                {
                    if (Widgets.CloseButtonFor(rect.AtZero()))
                    {
                        specificTrainingTabForpawn = null;
                        SoundDefOf.TabClose.PlayOneShotOnCamera();
                    }
                    rect.ContractedBy(15f);
                    Text.Font = GameFont.Small;
                    Listing_Standard listing_Standard = new Listing_Standard();
                    listing_Standard.Begin(rect.AtZero());
                    if (localSpecificTrainingTabForpawn.RaceProps.showTrainables)
                    {
                        listing_Standard.Gap();
                        List<TrainableDef> trainableDefsInListOrder = TrainableUtility.TrainableDefsInListOrder;
                        for (int i = 0; i < trainableDefsInListOrder.Count; i++)
                        {
                            TrainingCardUtility.TryDrawTrainableRow(listing_Standard, localSpecificTrainingTabForpawn, trainableDefsInListOrder[i]);
                        }
                    }
                    listing_Standard.End();

                }
            });
        }

        private void EnsureSpecificNeedsTabForPawnValid()
        {
            if (specificTrainingTabForpawn != null && (specificTrainingTabForpawn.Destroyed || !SelOutpost.Has(specificTrainingTabForpawn)))
            {
                specificTrainingTabForpawn = null;
            }
        }

        protected override void FillTab()
        {
            EnsureSpecificNeedsTabForPawnValid();
            DoRows(size, Pawns);
        }

        private void DoRow(ref float curY, Rect viewRect, Rect scrollOutRect, Pawn pawn)
        {
            float num = scrollPosition.y - 40f;
            float num2 = scrollPosition.y + scrollOutRect.height;
            if (curY > num && curY < num2)
            {
                DoRow(new Rect(0f, curY, viewRect.width, 40f), pawn);
            }
            curY += 40f;
        }

        private void DoRows(Vector2 size, List<Pawn> pawns)
        {
            if (specificTrainingTabForpawn != null && (!pawns.Contains(specificTrainingTabForpawn) || specificTrainingTabForpawn.Dead))
            {
                specificTrainingTabForpawn = null;
            }
            Text.Font = GameFont.Small;
            Rect rect = new Rect(0f, 0f, size.x, size.y).ContractedBy(10f);
            Rect viewRect = new Rect(0f, 0f, rect.width - 16f, scrollViewHeight);
            Widgets.BeginScrollView(rect, ref scrollPosition, viewRect);
            float curY = 0f;
            for (int i = 0; i < pawns.Count; i++)
            {
                Pawn pawn = pawns[i];
                DoRow(ref curY, viewRect, rect, pawn);
            }
            if (Event.current.type == EventType.Layout)
            {
                scrollViewHeight = curY + 30f;
            }
            Widgets.EndScrollView();
        }

        private void DoRow(Rect rect, Pawn pawn)
        {
            GUI.BeginGroup(rect);
            Rect rect2 = rect.AtZero();
            Widgets.InfoCardButton(rect2.width - 24f, (rect.height - 24f) / 2f, pawn);
            rect2.width -= 24f;
            if (!pawn.Dead)
            {
                CaravanThingsTabUtility.DoOpenSpecificTabButton(rect2, pawn, ref specificTrainingTabForpawn);
                rect2.width -= 24f;
                CaravanThingsTabUtility.DoOpenSpecificTabButtonInvisible(rect2, pawn, ref specificTrainingTabForpawn);
            }
            Widgets.DrawHighlightIfMouseover(rect2);
            Rect rect3 = new Rect(4f, (rect.height - 27f) / 2f, 27f, 27f);
            Widgets.ThingIcon(rect3, pawn);
            Rect bgRect = new Rect(rect3.xMax + 4f, 11f, 100f, 18f);
            GenMapUI.DrawPawnLabel(pawn, bgRect, 1f, 100f, null, GameFont.Small, alwaysDrawBg: false, alignCenter: false);
            if (pawn.Downed)
            {
                GUI.color = new Color(1f, 0f, 0f, 0.5f);
                Widgets.DrawLineHorizontal(0f, rect.height / 2f, rect.width);
                GUI.color = Color.white;
            }
            GUI.EndGroup();
        }

    }
}
