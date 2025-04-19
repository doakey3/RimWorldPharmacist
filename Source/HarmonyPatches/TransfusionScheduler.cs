using RimWorld;
using Verse;

namespace Pharmacist
{
    public class CompAutoTransfuse : ThingComp
    {
        public override void CompTickRare()
        {
            base.CompTickRare();

            if (parent is not Pawn pawn) return;
            if (!pawn.Spawned || pawn.Map == null || pawn.Dead) return;

            Hediff bloodLoss = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.BloodLoss);
            if (bloodLoss == null || bloodLoss.Severity < PharmacistSettings.hemogenThreshold)
                return;

            bool enableTransfusion = pawn.IsSlave ? PharmacistSettings.autoTransfuseSlave :
                pawn.IsColonist ? PharmacistSettings.autoTransfuseColonist :
                pawn.IsPrisoner ? PharmacistSettings.autoTransfusePrisoner :
                pawn.HostFaction != null ? PharmacistSettings.autoTransfuseGuest : false;

            if (!enableTransfusion)
                return;

            foreach (var existingBill in pawn.BillStack.Bills)
            {
                if (existingBill.recipe != null && existingBill.recipe.defName == "BloodTransfusion")
                    return;
            }

            RecipeDef recipe = DefDatabase<RecipeDef>.GetNamedSilentFail("BloodTransfusion");
            if (recipe == null)
                return;

            Bill_Medical newBill = new Bill_Medical(recipe, null);
            pawn.BillStack.AddBill(newBill);
        }
    }

    public class CompProperties_AutoTransfuse : CompProperties
    {
        public CompProperties_AutoTransfuse()
        {
            compClass = typeof(CompAutoTransfuse);
        }
    }
}
