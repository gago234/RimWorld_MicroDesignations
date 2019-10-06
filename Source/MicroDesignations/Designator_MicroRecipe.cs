using System;
using System.Collections.Generic;
using System.Linq;
using Harmony;
using Verse;
using RimWorld;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;


namespace MicroDesignations
{
    public class Designator_MicroRecipe : Designator
    {
        private RecipeDef recipeDef;
        public DesignationDef designationDef = null;
        public Designator_MicroRecipe(RecipeDef recipeDef, BuildableDef thingUser)
        {
            this.recipeDef = recipeDef;
        
            defaultLabel = recipeDef.label;
            defaultDesc = recipeDef.description;
            soundDragSustain = SoundDefOf.Designate_DragStandard;
            soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
            useMouseIcon = true;

            try
            {
                designationDef = DefDatabase<DesignationDef>.AllDefsListForReading.FirstOrDefault(x => x.defName == recipeDef.defName + "Designation");
            }
            catch { Log.Message($"weird thing happened, couldn't load DesignationDef for Designator({this})"); }

            order = 200f;

            icon = thingUser.uiIcon;
            iconAngle = thingUser.uiIconAngle;
            iconOffset = thingUser.uiIconOffset;

            ThingDef thingDef = thingUser as ThingDef;
            if (thingDef == null)
            {
                iconProportions = thingDef.graphicData.drawSize.RotatedBy(thingDef.defaultPlacingRot);
                iconDrawScale = GenUI.IconDrawScale(thingDef);
            } else
            {
                iconProportions = new Vector2(1f, 1f);
                iconDrawScale = 1f;
            }
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
            if (!t.Spawned || Map.designationManager.DesignationOn(t, Designation) != null)
                return false;

            if (t.def.comps.FirstOrDefault(x => x is CompProperties_ApplicableDesignation && (x as CompProperties_ApplicableDesignation).designationDef == designationDef) == null)
                return false;
            List<SpecialThingFilterDef> l = (List<SpecialThingFilterDef>)MicroDesignations.LdisallowedFilters.GetValue(recipeDef.fixedIngredientFilter);
            if (l != null)
                for (int i = 0; i < l.Count; i++)
                    if (l[i].Worker.Matches(t))
                        return false;

            return true;
        }

        public override void SelectedUpdate()
        {
            GenUI.RenderMouseoverBracket();
        }
    }
}
