using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using UnityEngine;


namespace MicroDesignations
{
    public class Designator_MicroRecipe : Designator
    {
        private RecipeDef recipeDef;
        private ThingDef stuffDef = null;
        private BuildableDef entDef;
        public DesignationDef designationDef = null;
        private static readonly Vector2 TerrainTextureCroppedSize = new Vector2(64f, 64f);
        private static readonly Vector2 DragPriceDrawOffset = new Vector2(19f, 17f);

        public Designator_MicroRecipe(RecipeDef recipeDef, BuildableDef thingUser)
        {
            this.recipeDef = recipeDef;

            defaultLabel = recipeDef.label;
            defaultDesc = recipeDef.description;
            soundDragSustain = SoundDefOf.Designate_DragStandard;
            soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
            soundSucceeded = SoundDefOf.Designate_Claim;
            //useMouseIcon = true;

            try { designationDef = DefDatabase<DesignationDef>.AllDefsListForReading.FirstOrDefault(x => x.defName == recipeDef.defName + "Designation"); }
            catch { Log.Message($"weird thing happened, couldn't load DesignationDef for Designator({this})"); }

            entDef = thingUser;
            icon = entDef.uiIcon;
            iconAngle = entDef.uiIconAngle;
            iconOffset = entDef.uiIconOffset;
            order = 200f;
            ThingDef thingDef = entDef as ThingDef;
            if (thingDef == null)
            {
                iconProportions = thingDef.graphicData.drawSize.RotatedBy(thingDef.defaultPlacingRot);
                iconDrawScale = GenUI.IconDrawScale(thingDef);
            } else
            {
                iconProportions = new Vector2(1f, 1f);
                iconDrawScale = 1f;
            }

            TerrainDef terrainDef = entDef as TerrainDef;
            if (terrainDef != null)
            {
                iconTexCoords = new Rect(0f, 0f, TerrainTextureCroppedSize.x / icon.width, TerrainTextureCroppedSize.y / icon.height);
            }

            ResetStuffToDefault();
        }

        public void ResetStuffToDefault()
        {
            ThingDef thingDef = entDef as ThingDef;
            if (thingDef != null && thingDef.MadeFromStuff)
            {
                stuffDef = GenStuff.DefaultStuffFor(thingDef);
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

        public override Color IconDrawColor
        {
            get
            {
                if (stuffDef != null)
                {
                    return stuffDef.stuffProps.color;
                }
                return entDef.uiIconColor;
            }
        }

        public override void SelectedUpdate()
        {
            GenUI.RenderMouseoverBracket();
        }
    }
}
