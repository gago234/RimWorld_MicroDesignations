using System.Collections.Generic;
using System.Linq;
using Verse;
using HarmonyLib;
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

                foreach (var rec in list.Where(x => x.AllRecipeUsers != null
                    && x.AllRecipeUsers.FirstOrDefault(y => y is BuildableDef) != null
                    && !x.ingredients.NullOrEmpty() && x.ingredients.Count() == 1
                    && x.ingredients[0].filter != null
                    && x.ingredients[0].filter.AnyAllowedDef != null
                    && x.ingredients[0].filter.AnyAllowedDef.stackLimit < 2
                    && x.fixedIngredientFilter != null
                    && x.fixedIngredientFilter.AllowedThingDefs != null))
                {
                    foreach (var rUser in rec.AllRecipeUsers.Where(y => y is BuildableDef))
                    {
                        IEnumerable<WorkGiverDef> gListSel = gList.Where(x => !x.fixedBillGiverDefs.NullOrEmpty()
                           && x.fixedBillGiverDefs.Contains(rUser) && (x.giverClass == typeof(WorkGiver_DoBill) || x.giverClass.IsSubclassOf(typeof(WorkGiver_DoBill))));

                        if (gListSel == null)
                            continue;

                        WorkGiverDef wGiverDef = gListSel.FirstOrDefault();

                        if (wGiverDef == null)
                            continue;

                        DesignationDef dDef = DefDatabase<DesignationDef>.AllDefsListForReading.FirstOrDefault(x => x.defName == rec.defName + "Designation");
                        if (dDef == null)
                        {
                            dDef = new DesignationDef()
                            {
                                defName = rec.defName + "Designation",
                                texturePath = "Designations/Uninstall",
                                iconMat = MaterialPool.MatFrom("Designations/Uninstall", ShaderDatabase.MetaOverlay),
                                targetType = TargetType.Thing
                            };
                            DefDatabase<DesignationDef>.Add(dDef);
                        }

                        foreach (var def in rec.fixedIngredientFilter.AllowedThingDefs)
                            if(def.comps != null)
                                def.comps.Add(new CompProperties_ApplicableDesignation() {designationDef = dDef} );

                        string wgname = $"{wGiverDef.defName}_{rec.defName}_DesignationWorkGiver";

                        WorkGiverDef wgDef = null;
                        gListSel = gList.Where(x => x.defName == wgname);

                        if (gListSel != null)
                            wgDef = gListSel.FirstOrDefault();

                        if (wgDef == null && wGiverDef.workType != null && wGiverDef.requiredCapacities != null)
                        {
                            JobDef jDef = JobDefOf.DoBill;
                            
                            if (wGiverDef.giverClass != typeof(WorkGiver_DoBill))
                            {
                                RecipeJobDef rj = DefDatabase<RecipeJobDef>.AllDefsListForReading.FirstOrDefault(x => x.defName == rec.defName);
                                if (rj == null)
                                {
                                    rj = DefDatabase<RecipeJobDef>.AllDefsListForReading.FirstOrDefault(x => x.workerClass == rec.workerClass);
                                }

                                if (rj == null)
                                {
                                    Log.Message($"Couldn't find a proper RecipeJobDef for redefined DoBill task {wGiverDef.defName}");
                                    continue;
                                }

                                jDef = DefDatabase<JobDef>.AllDefsListForReading.FirstOrDefault(x => x.defName == rj.jobName);
                                if(jDef == null)
                                {
                                    Log.Message($"Couldn't find a JobDef({rj.jobName}) for RecipeJobDef({rj.defName})");
                                    continue;
                                }
                            }
                            wgDef = new MicroWorkGiverDef()
                            {
                                recipeDef = rec,
                                designationDef = dDef,
                                defName = wgname,
                                label = wGiverDef.label,
                                giverClass = typeof(WorkGiver_MicroRecipe),
                                jobDef = jDef,
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
