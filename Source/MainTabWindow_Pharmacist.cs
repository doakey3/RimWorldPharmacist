// Window_Pharmacist.cs
// Copyright Karel Kroeze, 2018-2018

using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;
using static Pharmacist.Constants;
using static Pharmacist.Resources;

namespace Pharmacist {
    [StaticConstructorOnStartup]
    public class MainTabWindow_Pharmacist: MainTabWindow {
        public override Vector2 InitialSize => new Vector2(
            CareSelectorWidth + OptionsWidth + Constants.Margin + (2 * Margin),
            TitleHeight + CareSelectorHeight + (2 * Margin));
        internal static Population[] populations = Enum.GetValues( typeof( Population ) ).Cast<Population>().ToArray();
        internal static InjurySeverity[] severities = Enum.GetValues( typeof( InjurySeverity ) ).Cast<InjurySeverity>().ToArray();
        internal static MedicalCareCategory[] medcares = Enum.GetValues( typeof( MedicalCareCategory ) ).Cast<MedicalCareCategory>().ToArray();

        private static readonly Texture2D HemogenIcon = ContentFinder<Texture2D>.Get("UI/Icons/hemogen", true);
        private static readonly Texture2D CheckOnIcon  = ContentFinder<Texture2D>.Get("UI/Icons/CheckOn", true);
        private static readonly Texture2D CheckOffIcon = ContentFinder<Texture2D>.Get("UI/Icons/CheckOff", true);

        internal static int CareSelectorWidth => CareSelectorRowLabelWidth + (CareSelectorColumnWidth * severities.Length) + CareSelectorColumnWidth;
        internal static int CareSelectorHeight => RowHeight * (populations.Length + 1);
        internal static int OptionsWidth => 300;
        internal static int OptionsHeight => CareSelectorHeight;

        public override void DoWindowContents(Rect canvas) {
            if (PharmacistSettings.medicalCare == null) {
                PharmacistSettings.SetDefaults();
            }

            Rect titleRect = new Rect(
                canvas.xMin,
                canvas.yMin,
                canvas.width,
                TitleHeight );
            Rect careSelectorRect = new Rect(
                canvas.xMin,
                titleRect.yMax,
                CareSelectorWidth,
                CareSelectorHeight );
            Rect optionsRect = new Rect(
                careSelectorRect.xMax + Constants.Margin,
                titleRect.yMax,
                OptionsWidth,
                OptionsHeight );

            Text.Font = GameFont.Medium;
            Widgets.Label(titleRect, "Fluffy.Pharmacist.Settings.Title".Translate());
            Text.Font = GameFont.Small;

            DrawCareSelectors(careSelectorRect);
            DrawOptions(optionsRect);
        }

        private void CreateMedicalCareSelectionFloatMenu(Action<MedicalCareCategory> action) {
            List<FloatMenuOption> options = new List<FloatMenuOption>();
            foreach (MedicalCareCategory category in medcares) {
                options.Add(new FloatMenuOption($"MedicalCareCategory_{category}".Translate(),
                    () => action(category),
                    extraPartWidth: 30,
                    extraPartOnGUI: rect => {
                        Rect optionIconRect = new Rect( 0f, 0f, IconSize, IconSize )
                            .CenteredOnXIn( rect )
                            .CenteredOnYIn( rect );
                        GUI.DrawTexture(optionIconRect, medcareGraphics[(int) category]);
                        return false;
                    }));
            }

            Find.WindowStack.Add(new FloatMenu(options));
        }

        private float _optionsHeight = -1f;
        private Vector2 _optionsScrollPosition = Vector2.zero;
        private void DrawOptions(Rect canvas) {
            GUI.DrawTexture(canvas, SlightlyDarkBackground);

            Rect viewRect = new Rect(
                canvas.xMin,
                canvas.yMin,
                canvas.width - 16f,
                _optionsHeight );

            Rect row = new Rect(
                viewRect.xMin + Constants.Margin,
                viewRect.yMin + Constants.Margin,
                viewRect.width - (Constants.Margin * 2),
                RowHeight );

            Widgets.BeginScrollView(canvas, ref _optionsScrollPosition, viewRect);

            Widgets.Label(row, "Fluffy.Pharmacist.DiseaseMargin".Translate(PharmacistSettings.medicalCare.DiseaseMargin.ToStringPercent()));
            TooltipHandler.TipRegion(row, "Fluffy.Pharmacist.DiseaseMargin.Tip".Translate());
            row.y += RowHeight;
            PharmacistSettings.medicalCare.DiseaseMargin = Widgets.HorizontalSlider(row, PharmacistSettings.medicalCare.DiseaseMargin, 0f, 1f, roundTo: .01f);
            row.y += RowHeight;

            Widgets.Label(row, "Fluffy.Pharmacist.DiseaseThreshold".Translate(PharmacistSettings.medicalCare.DiseaseThreshold.ToStringPercent()));
            TooltipHandler.TipRegion(row, "Fluffy.Pharmacist.DiseaseThreshold.Tip".Translate());
            row.y += RowHeight;
            PharmacistSettings.medicalCare.DiseaseThreshold = Widgets.HorizontalSlider(row, PharmacistSettings.medicalCare.DiseaseThreshold, 0f, 1f, roundTo: .01f);
            row.y += RowHeight;

            Widgets.Label(row, "Fluffy.Pharmacist.MinorWoundsThreshold".Translate(PharmacistSettings.medicalCare.MinorWoundsThreshold));
            TooltipHandler.TipRegion(row, "Fluffy.Pharmacist.MinorWoundsThreshold.Tip".Translate());
            row.y += RowHeight;
            PharmacistSettings.medicalCare.MinorWoundsThreshold = (int) Widgets.HorizontalSlider(row, PharmacistSettings.medicalCare.MinorWoundsThreshold, 2, 20, roundTo: 1);
            row.y += RowHeight;

            Widgets.Label(row, "Fluffy.Pharmacist.TransfusionThreshold".Translate(PharmacistSettings.hemogenThreshold.ToStringPercent()));
            TooltipHandler.TipRegion(row, "Fluffy.Pharmacist.TransfusionThreshold.Tip".Translate());
            row.y += RowHeight;
            PharmacistSettings.hemogenThreshold = Widgets.HorizontalSlider(row, PharmacistSettings.hemogenThreshold, 0f, 1f, roundTo: .01f);
            row.y += RowHeight;

            Widgets.EndScrollView();
            _optionsHeight = row.yMax - canvas.yMin;
        }

        private void DrawCareSelectors(Rect canvas) {
            GUI.DrawTexture(canvas, SlightlyDarkBackground);

            Vector2 pos = new Vector2( canvas.xMin + CareSelectorRowLabelWidth, canvas.yMin );
            foreach (InjurySeverity severity in severities) {
                Rect cell = new Rect( pos.x, pos.y, CareSelectorColumnWidth, RowHeight );
                Rect headerIconRect = new Rect( 0, 0, IconSize, IconSize )
                    .CenteredOnXIn( cell )
                    .CenteredOnYIn( cell );

                TooltipHandler.TipRegion(cell,
                    $"Fluffy.Pharmacist.Severity.{severity}".Translate() + "\n\n" +
                    $"Fluffy.Pharmacist.Severity.{severity}.Tip".Translate());
                GUI.DrawTexture(headerIconRect, severityTextures[(int) severity]);

                Widgets.DrawHighlightIfMouseover(cell);
                if (Widgets.ButtonInvisible(cell)) {
                    CreateMedicalCareSelectionFloatMenu(category => {
                        foreach (Population population in populations) {
                            PharmacistSettings.medicalCare[population][severity] = category;
                        }
                    });
                }

                pos.x += CareSelectorColumnWidth;
            }

            // Hemogen column header
            Rect hemogenHeaderCell = new Rect(pos.x, pos.y, CareSelectorColumnWidth, RowHeight);
            Rect hemogenIconRect = new Rect(0, 0, IconSize, IconSize)
                .CenteredOnXIn(hemogenHeaderCell)
                .CenteredOnYIn(hemogenHeaderCell);
            TooltipHandler.TipRegion(hemogenHeaderCell, "Toggle automatic hemogen transfusions for this population.");
            GUI.DrawTexture(hemogenIconRect, HemogenIcon);
            pos.x += CareSelectorColumnWidth;

            pos.x = canvas.xMin;
            pos.y += RowHeight;

            foreach (Population population in populations) {
                Rect populationLabelRect = new Rect( pos.x, pos.y, CareSelectorRowLabelWidth, RowHeight );
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(populationLabelRect, $"Fluffy.Pharmacist.Population.{population}".Translate());
                Text.Anchor = TextAnchor.UpperLeft;

                Widgets.DrawHighlightIfMouseover(populationLabelRect);
                if (Widgets.ButtonInvisible(populationLabelRect)) {
                    CreateMedicalCareSelectionFloatMenu(category => {
                        foreach (InjurySeverity severity in severities) {
                            PharmacistSettings.medicalCare[population][severity] = category;
                        }
                    });
                }

                pos.x += CareSelectorRowLabelWidth;

                foreach (InjurySeverity severity in severities) {
                    Rect cell = new Rect(
                        pos.x,
                        pos.y,
                        CareSelectorColumnWidth,
                        RowHeight );
                    Rect iconRect = new Rect( 0, 0, IconSize, IconSize )
                        .CenteredOnXIn( cell )
                        .CenteredOnYIn( cell );

                    Widgets.DrawHighlightIfMouseover(cell);
                    GUI.DrawTexture(iconRect, medcareGraphics[(int) PharmacistSettings.medicalCare[population][severity]]);

                    if (Widgets.ButtonInvisible(cell)) {
                        CreateMedicalCareSelectionFloatMenu(category => PharmacistSettings.medicalCare[population][severity] = category);
                    }

                    pos.x += CareSelectorColumnWidth;
                }

                // Hemogen transfusion toggle
                Rect hemogenCell = new Rect(pos.x, pos.y, CareSelectorColumnWidth, RowHeight);
                Rect checkIconRect = new Rect(0, 0, IconSize, IconSize)
                    .CenteredOnXIn(hemogenCell)
                    .CenteredOnYIn(hemogenCell);

                bool enabled = GetHemogenToggle(population);
                Texture2D checkIcon = enabled ? CheckOnIcon : CheckOffIcon;
                GUI.DrawTexture(checkIconRect, checkIcon);

                Widgets.DrawHighlightIfMouseover(hemogenCell);
                if (Widgets.ButtonInvisible(hemogenCell)) {
                    SetHemogenToggle(population, !enabled);
                    if (enabled) {
                        SoundStarter.PlayOneShotOnCamera(SoundDefOf.Checkbox_TurnedOn);
                    } else {
                        SoundStarter.PlayOneShotOnCamera(SoundDefOf.Checkbox_TurnedOff);
                    }
                }

                pos.x += CareSelectorColumnWidth;
                pos.x = canvas.xMin;
                pos.y += RowHeight;
            }
        }

        private bool GetHemogenToggle(Population pop)
        {
            return pop switch
            {
                Population.Colonist => PharmacistSettings.autoTransfuseColonists,
                Population.Prisoner => PharmacistSettings.autoTransfusePrisoners,
                Population.Guest => PharmacistSettings.autoTransfuseGuests,
                _ => false
            };
        }

        private void SetHemogenToggle(Population pop, bool enabled)
        {
            switch (pop)
            {
                case Population.Colonist:
                    PharmacistSettings.autoTransfuseColonists = enabled;
                    break;
                case Population.Prisoner:
                    PharmacistSettings.autoTransfusePrisoners = enabled;
                    break;
                case Population.Guest:
                    PharmacistSettings.autoTransfuseGuests = enabled;
                    break;
            }
        }
    }
}
