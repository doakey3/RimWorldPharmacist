using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Pharmacist.HarmonyPatches {
    [HarmonyPatch(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.HealthTick))]
    public static class Patch_Pawn_HealthTick_TransfusionScheduler {
        public static void Postfix(Pawn_HealthTracker __instance) {
            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            if (pawn == null || !pawn.Spawned || pawn.Map == null || pawn.Dead)
                return;

            Hediff bloodLoss = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.BloodLoss);
            if (bloodLoss == null || bloodLoss.Severity < PharmacistSettings.hemogenThreshold)
                return;

            bool enableTransfusion = false;
            if (pawn.IsColonist) {
                enableTransfusion = PharmacistSettings.autoTransfuseColonists;
            } else if (pawn.IsPrisoner) {
                enableTransfusion = PharmacistSettings.autoTransfusePrisoners;
            } else if (pawn.HostFaction != null && !pawn.IsPrisoner) {
                enableTransfusion = PharmacistSettings.autoTransfuseGuests;
            }

            if (!enableTransfusion)
                return;

            foreach (var existingBill in pawn.BillStack.Bills) {
                if (existingBill.recipe != null && existingBill.recipe.defName == "BloodTransfusion") {
                    return;
                }
            }

            RecipeDef recipe = DefDatabase<RecipeDef>.GetNamedSilentFail("BloodTransfusion");
            if (recipe == null)
                return;

            Bill_Medical newBill = new Bill_Medical(recipe, null);
            pawn.BillStack.AddBill(newBill);
        }
    }
}
