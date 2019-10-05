using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Harmony;
using RimWorld;

namespace MicroDesignations
{
    public static class StaticConstructorOnStartupUtility_Patch
    {
        [HarmonyPatch(typeof(StaticConstructorOnStartupUtility), "CallAll")]
        static class StaticConstructorOnStartupUtility_CallAll_MicroDesignationsPatch
        {
            static void Postfix()
            {
                List<RecipeDef> list = DefDatabase<RecipeDef>.AllDefsListForReading;
                List<WorkGiverDef> gList = DefDatabase<WorkGiverDef>.AllDefsListForReading;

                foreach (var rec in list.Where(x => x.AllRecipeUsers.FirstOrDefault(y => y is BuildableDef) != null && x.ingredients.Count() == 1 && x.ingredients[0].filter.AnyAllowedDef.stackLimit < 2))
                {
                    foreach (var rUser in rec.AllRecipeUsers.Where(y => y is BuildableDef))
                    {
                        WorkGiverDef wGiverDef = gList.Where(x => !x.fixedBillGiverDefs.NullOrEmpty() && x.fixedBillGiverDefs.Contains(rUser) && x.giverClass == typeof(WorkGiver_DoBill)).FirstOrDefault();
                        if (wGiverDef == null)
                            continue;

                        DesignationDef dDef = DefDatabase<DesignationDef>.AllDefsListForReading.FirstOrDefault(x => x.defName == rec.defName + "Designation");
                        if (dDef == null)
                        {
                            //Log.Message(rec.defName + "Designation");
                            dDef = new DesignationDef()
                            {
                                defName = rec.defName + "Designation",
                                texturePath = "Designations/Uninstall",
                                iconMat = MaterialPool.MatFrom("Designations/Uninstall", ShaderDatabase.MetaOverlay),
                                targetType = TargetType.Thing
                            };
                            DefDatabase<DesignationDef>.Add(dDef);
                        }

                        foreach(var def in rec.fixedIngredientFilter.AllowedThingDefs)
                            def.comps.Add(new CompProperties_ApplicableDesignation(dDef));

                        string wgname = $"{wGiverDef.defName}_{rec.defName}_DesignationWorkGiver";
                        WorkGiverDef wgDef = gList.Where(x => x.defName == wgname).FirstOrDefault();
                        if (wgDef == null)
                        {
                            wgDef = new MicroWorkGiverDef()
                            {
                                recipeDef = rec,
                                designationDef = dDef,
                                defName = wgname,
                                label = wGiverDef.label,
                                giverClass = typeof(WorkGiver_MicroRecipe),
                                workType = wGiverDef.workType,
                                priorityInType = wGiverDef.priorityInType - 5,
                                verb = wGiverDef.verb,
                                gerund = wGiverDef.verb,//wGiverDef.gerund,
                                requiredCapacities = new List<PawnCapacityDef>(wGiverDef.requiredCapacities),
                                prioritizeSustains = wGiverDef.prioritizeSustains
                            };
                            DefDatabase<WorkGiverDef>.Add(wgDef);
                            wgDef.workType.workGiversByPriority.Add(wgDef);
                        }
                    }
                }
            }
        }
    }
}
