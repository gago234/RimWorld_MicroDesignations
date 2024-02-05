using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using VFEAncients;
using VFEAncients.HarmonyPatches;

namespace MicroDesignations.ModCompatability
{

    [StaticConstructorOnStartup]
    public static class AncientsUtility
    {
        static AncientsUtility()
        {
            if (ModLister.GetActiveModWithIdentifier("vanillaexpanded.vfea") != null)
            {
                AncientsCompat.IsActive = true;
            }
        }
        public static bool HasAncientsExtension(RecipeDef recipe)
        {
            if (AncientsCompat.IsActive)
                return AncientsCompat.HasAncientsExtension(recipe);
            else
                return false;
        }

        public static bool IsUsable(Thing t,RecipeDef recipe)
        { 
            if (!t.def.useHitPoints || (!t.def.IsWeapon && !t.def.IsApparel))
            {
                return recipe.ingredients[0].filter.AllowedThingDefs.Any(IsStuffIngredient(t));
            }
            else 
                return t.HitPoints < t.MaxHitPoints;
        }

        public static Func<ThingDef, bool> IsStuffIngredient(Thing t)
        {
            if (AncientsCompat.IsActive)
                return AncientsCompat.IsStuffIngredient(t);
            else
                return null;
        }

    }

    public static class AncientsCompat
    {
        public static bool IsActive;
        public static bool HasAncientsExtension(RecipeDef recipe)
        {
            return recipe.HasModExtension<RecipeExtension_Mend>();
        }
        public static Func<ThingDef, bool> IsStuffIngredient(Thing t)
        {
            return MendingPatches.IsStuffIngredient(t);
        }

        public static ThingDef NonStuffStuff(ThingDef def)
        {
            return MendingPatches.NonStuffStuff(def);
        }

    }

}
