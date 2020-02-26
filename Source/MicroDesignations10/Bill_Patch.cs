using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Harmony;
using RimWorld;

namespace MicroDesignations
{
    class Bill_Patch
    {
        [HarmonyPatch(typeof(Bill), nameof(Bill.DeletedOrDereferenced), MethodType.Getter)]
        static class Bill_DeletedOrDereferenced_MicroDesignationsPatch
        {
            static bool Prefix(Bill __instance, ref bool __result)
            {
                if(__instance.billStack == null)
                {
                    __result = __instance.deleted;
                    return false;
                }
                return true;
            }
        }
        /*
        [HarmonyPatch(typeof(GenRecipe), nameof(GenRecipe.MakeRecipeProducts))]
        static class GenRecipe_MakeRecipeProducts_MicroDesignationsPatch
        {
            static void Prefix(RecipeDef recipeDef, Pawn worker, List<Thing> ingredients, Thing dominantIngredient, IBillGiver billGiver)
            {
                foreach (var ing in ingredients)
                {
                    Log.Message($"{ing}");
                }
            }
        }
        */
    }
}
