using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace MicroDesignations.Patches
{
    /*
    [HarmonyPatch]
    static class WorkPriority_get_workGiver_MicroDesignatorsPatch
    {
        static Type TWorkPriority = null;
        static MethodBase Lget_Workgiver = null;

        internal static bool Prepare()
        {
            TWorkPriority = AccessTools.TypeByName("WorkTab.WorkPriority");
            if (TWorkPriority != null) Lget_Workgiver = AccessTools.PropertyGetter(TWorkPriority, "Workgiver");
            else return false;

            return true;
        }

        internal static MethodBase TargetMethod()
        {
            //Log.Message($"targetmethod={LPawnPriorityTracker}");
            return Lget_Workgiver;
        }

        static void Postfix(ref WorkGiverDef __result)
        {
            //Log.Message($"before={__result}");
            if (Scribe.mode == LoadSaveMode.PostLoadInit) return;
            if (__result is MicroWorkGiverDef) __result = (__result as MicroWorkGiverDef).workGiverDef;
            Log.Message($"after={__result}");
        }
    }
    */

    
    [HarmonyPatch]
    static class PawnPriorityTracker_ctor_MicroDesignatorsPatch
    {
        static Type TPawnPriorityTracker = null;
        static MethodBase Lget_Item = null;
        //static FieldInfo Lpriorities = null;

        //static Type TWorkPriority = null;
        //static FieldInfo Lworkgiver = null;

        internal static bool Prepare()
        {
            TPawnPriorityTracker = AccessTools.TypeByName("WorkTab.PawnPriorityTracker");
            if (TPawnPriorityTracker != null) Lget_Item = TPawnPriorityTracker.GetMethod("get_Item");
            else return false;

            return true;
        }

        internal static MethodBase TargetMethod()
        {
            //Log.Message($"targetmethod={Lget_Item}");
            return Lget_Item;
        }

        static void Prefix(ref WorkGiverDef workgiver)
        {
            if (workgiver is MicroWorkGiverDef) workgiver = (workgiver as MicroWorkGiverDef).workGiverDef;
        }
    }
    
}
