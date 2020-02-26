using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Harmony;
using RimWorld;
using Verse.AI;

namespace MicroDesignations
{
    static class Job_Patch
    {
        [HarmonyPatch(typeof(Job), nameof(Job.ExposeData))]
        static class Job_ExposeData_MicroDesignationsPatch
        {
            static void Prefix(Job __instance)
            {
                if (__instance.bill == null || __instance.bill.billStack == null) Scribe_Deep.Look(ref __instance.bill, "billSaved"); 
            }
        }
    }
}
