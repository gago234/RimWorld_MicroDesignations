using System.Linq;
using UnityEngine;
using RimWorld;
using Verse;

namespace MicroDesignations
{
    public class Designator_MicroRecipe : Designator
    {
        private RecipeDef recipeDef;
        private ThingFilter ingFilter;
        private DesignationDef designationDef = null;
        public Designator_MicroRecipe(RecipeDef recipeDef, BuildableDef thingUser)
        {
            this.recipeDef = recipeDef;

            if (recipeDef.ingredients[0].IsFixedIngredient)
                ingFilter = recipeDef.ingredients[0].filter;
            else
            {
                Bill tmp = recipeDef.MakeNewBill();
                ingFilter = tmp.ingredientFilter;
            }

            defaultLabel = recipeDef.label;
            defaultDesc = recipeDef.description;
            soundDragSustain = SoundDefOf.Designate_DragStandard;
            soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
            useMouseIcon = true;

            designationDef = DefDatabase<DesignationDef>.AllDefsListForReading.First(x => x.defName == recipeDef.defName + "Designation");

            order = 200f;
            Designator_Build des = BuildCopyCommandUtility.FindAllowedDesignator(thingUser, true);
            if (des == null)
                return;

            icon = des.icon;
            iconProportions = des.iconProportions;
            iconDrawScale = des.iconDrawScale;
            iconTexCoords = des.iconTexCoords;
            iconAngle = des.iconAngle;
            iconOffset = des.iconOffset;
        }

        public override int DraggableDimensions
        {
            get
            {
                return 2;
            }
        }

        protected override DesignationDef Designation
        {
            get
            {
                return designationDef;
            }
        }

        private Thing TopDesignatableThing(IntVec3 loc)
        {
            foreach (Thing thing in from t in Map.thingGrid.ThingsAt(loc)
                                    orderby t.def.altitudeLayer descending
                                    select t)
            {
                if (CanDesignateThing(thing).Accepted)
                {
                    return thing;
                }
            }
            return null;
        }

        public override AcceptanceReport CanDesignateCell(IntVec3 c)
        {
            if (!c.InBounds(Map))
            {
                return false;
            }
            if (c.Fogged(Map))
            {
                return false;
            }
            if (TopDesignatableThing(c) == null)
            {
                return false;
            }
            return true;
        }

        public override void DesignateSingleCell(IntVec3 loc)
        {
            DesignateThing(TopDesignatableThing(loc));
        }

        public override void DesignateThing(Thing t)
        {
            Map.designationManager.AddDesignation(new Designation(t, Designation));
        }

        public override AcceptanceReport CanDesignateThing(Thing t)
        {
            if (Map.designationManager.DesignationOn(t, Designation) != null)
            {
                return false;
            }

            bool b = false;

            foreach(var user in recipeDef.AllRecipeUsers)
            {
                if(Map.listerBuildings.ColonistsHaveBuilding(user))
                {
                    b = true;
                    break;
                }
            }
            if (!b)
                return false;

            if(ingFilter.Allows(t))
                return true;

            return false;
        }

        public override void SelectedUpdate()
        {
            GenUI.RenderMouseoverBracket();
        }
    }
}
