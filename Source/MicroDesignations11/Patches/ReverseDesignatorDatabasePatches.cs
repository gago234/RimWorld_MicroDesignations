using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;


namespace MicroDesignations
{
    public static class ReverseDesignatorDatabase_Patch
    {
        [HarmonyPatch(typeof(ReverseDesignatorDatabase), "InitDesignators")]
        static class ReverseDesignatorDatabase_InitDesignators_MicroDesignatorsPatch
        {
            static FieldInfo LdesList = null;
            static bool Prepare()
            {

                LdesList = AccessTools.Field(typeof(ReverseDesignatorDatabase), "desList");
                if (LdesList == null)
                    throw new Exception("Can't get field ReverseDesignatorDatabase.desList");

                return true;
            }

            static void Postfix(ReverseDesignatorDatabase __instance)
            {
                List<Designator> desList = (List<Designator>)LdesList.GetValue(__instance);
                List<RecipeDef> list = DefDatabase<RecipeDef>.AllDefsListForReading;

                foreach (var rec in list.Where(x => x.AllRecipeUsers.FirstOrDefault(y => y is BuildableDef) != null && x.ingredients.Count() == 1 && x.ingredients[0].filter.AnyAllowedDef.stackLimit < 2))
                {
                    Designator_MicroRecipe derp = new Designator_MicroRecipe(rec, rec.AllRecipeUsers.FirstOrDefault(y => y is BuildableDef));
                    if(derp.designationDef != null)
                        desList.Add(derp);
                }
            }
        }

    }
}
