using Verse;
using HarmonyLib;
using Verse.AI;
using RimWorld;
using System.Collections.Generic;
using System.Text;

namespace MicroDesignations.Patches
{
    /*[HarmonyPatch(typeof(ReservationManager), "LogCouldNotReserveError")]
    static class TEST_Patch
    {
        static void Prefix(Pawn claimant, Job job, LocalTargetInfo target, int maxPawns, int stackCount, ReservationLayerDef layer)
        {           
            if(job?.workGiverDef != null)
                Log.Message($"[BS] <color=green>workGiverDef</color>: {job.workGiverDef}");
        }

    }*/
}
