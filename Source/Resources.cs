// Resources.cs
// Copyright Karel Kroeze, 2018-2018

using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pharmacist {
    [StaticConstructorOnStartup]
    public static class Resources {
        public static Texture2D[] severityTextures;
        public static Texture2D pharmacistIcon;
        public static Texture2D[] medcareGraphics = AccessTools.Field( typeof( MedicalCareUtility ), "careTextures" ).GetValue( null ) as Texture2D[];
        public static Texture2D SlightlyDarkBackground = SolidColorMaterials.NewSolidColorTexture( new Color( 0f, 0f, 0f, .1f ) );

        static Resources() {
            severityTextures = new[]
            {
                ContentFinder<Texture2D>.Get( "UI/Icons/finger" ),
                ContentFinder<Texture2D>.Get( "UI/Icons/blood" ),
                ContentFinder<Texture2D>.Get( "UI/Icons/heart" ),
                ContentFinder<Texture2D>.Get( "UI/Icons/scalpel" ),
            };
            pharmacistIcon = ContentFinder<Texture2D>.Get("UI/Icons/hospital");
        }
    }
}
