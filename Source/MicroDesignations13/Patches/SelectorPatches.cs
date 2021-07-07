using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;
using System;


namespace MicroDesignations.Patches
{
    [HarmonyPatch(typeof(Selector), "ClearSelection")]
    static class Selector_ClearSelection_MicroDesignationsPatch
    {
        static void Postfix()
        {
            var now = DateTime.Now;
            Settings.lastSelectTick = now.Hour * 360000 + now.Minute * 60000 + now.Second * 1000 + now.Millisecond;
        }
    }

    [HarmonyPatch(typeof(Selector), "Deselect")]
    static class Selector_Deselect_MicroDesignationsPatch
    {
        static void Postfix()
        {
            var now = DateTime.Now;
            Settings.lastSelectTick = now.Hour * 360000 + now.Minute * 60000 + now.Second * 1000 + now.Millisecond;
        }
    }

    [HarmonyPatch(typeof(Selector), "Select")]
    static class Selector_Select_MicroDesignationsPatch
    {
        static void Postfix()
        {
            var now = DateTime.Now;
            Settings.lastSelectTick = now.Hour * 360000 + now.Minute * 60000 + now.Second * 1000 + now.Millisecond;
        }
    }
}
