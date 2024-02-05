using Verse;
using HarmonyLib;
using Verse.AI;
using RimWorld;
using System.Collections.Generic;
using System.Text;

namespace MicroDesignations.Patches
{
    /*[HarmonyPatch(typeof(WorkGiver_DoBill), "TryFindBestBillIngredients")]
    static class TEST_Patch
    {
        static void Prefix(Bill bill, Pawn pawn, Thing billGiver, List<ThingCount> chosen, List<IngredientCount> missingIngredients)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(" (");
            if(bill != null)
                sb.Append($"bill: {bill}, ");
            if (pawn != null)
                sb.Append($"pawn: {pawn}, ");
            if (billGiver != null)
                sb.Append($"billGiver: {billGiver}, ");
            sb.Append($"chosen: ");
            if (!chosen.NullOrEmpty())
                foreach (var x in chosen)
                    sb.Append($"<color=orange>{x}</color>, ");
            sb.Append($"missingIngredients: ");
            if (!missingIngredients.NullOrEmpty())
                foreach (var x in missingIngredients)
                    sb.Append($"<color=orange>{x}</color>, ");
            sb.Append(") ");
            Log.Message($"[BS] <color=green>PREFIX</color>: {sb}");
        }
        static void Postfix(Bill bill, Pawn pawn, Thing billGiver, List<ThingCount> chosen, List<IngredientCount> missingIngredients)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(" (");
            if (bill != null)
                sb.Append($"bill: {bill}, ");
            if (pawn != null)
                sb.Append($"pawn: {pawn}, ");
            if (billGiver != null)
                sb.Append($"billGiver: {billGiver}, ");
            sb.Append($"chosen: ");
            if (!chosen.NullOrEmpty())
                foreach (var x in chosen)
                    sb.Append($"<color=orange>{x}</color>, ");
            sb.Append($"missingIngredients: ");
            if (!missingIngredients.NullOrEmpty())
                foreach (var x in missingIngredients)
                    sb.Append($"<color=orange>{x}</color>, ");
            sb.Append(") ");
            Log.Message($"[BS] <color=yellow>POSTFIX</color>: {sb}");
        }
    }*/
}
