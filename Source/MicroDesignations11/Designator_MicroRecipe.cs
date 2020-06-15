using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using UnityEngine;


namespace MicroDesignations
{
    public class Designator_MicroRecipe : Action_Designator
    {
        private RecipeDef recipeDef;
        public DesignationDef designationDef = null;
        private static readonly Vector2 TerrainTextureCroppedSize = new Vector2(64f, 64f);
        private static readonly Vector2 DragPriceDrawOffset = new Vector2(19f, 17f);
        private Thing cachedThing = null;
        private bool cachedResult = false;
        private bool reloadBuildable = false;
        private BuildableDef cachedBuildable = null;
        private ThingDef cachedStuff = null;
        private bool cachedResearched = false;

        public Designator_MicroRecipe(RecipeDef recipeDef, BuildableDef thingUser)
        {
            this.recipeDef = recipeDef;

            defaultLabel = recipeDef.label;
            defaultDesc = recipeDef.description;
            soundDragSustain = SoundDefOf.Designate_DragStandard;
            soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
            soundSucceeded = SoundDefOf.Designate_Claim;

            try { designationDef = DefDatabase<DesignationDef>.AllDefsListForReading.FirstOrDefault(x => x.defName == recipeDef.defName + "Designation"); }
            catch { Log.Message($"weird thing happened, couldn't load DesignationDef for Designator({this})"); }

            order = 200f;
            icon = ContentFinder<Texture2D>.Get("UI/Empty", true);
        }

        public override Command_Action init_Command_Action()
        {
            //BuildableDef b;
            //ThingDef t;
            FindBuilding();
            
            if (cachedBuildable == null)
            {
                return base.init_Command_Action();
            }

            BuildableCommand_Action action = new BuildableCommand_Action();
            action.buildableDef = cachedBuildable;
            action.thingDef = cachedStuff;
            return action;
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
            if (cachedThing != null && cachedThing.def == t.def)
                return cachedResult;

            cachedThing = t;
            reloadBuildable = true;

            if (Settings.hide_unresearched && !recipeDef.AvailableNow)
                return cachedResult = false;

            if (Settings.hide_empty || Settings.hide_inactive)
            {
                //BuildableDef buildable;
                //ThingDef thing;
                FindBuilding();
                if (Settings.hide_empty && cachedBuildable == null || Settings.hide_inactive && !cachedResearched)
                    return cachedResult = false;
            }

            if (!t.Spawned || Map.designationManager.DesignationOn(t, Designation) != null)
                return cachedResult = false;

            if (t.def.comps.FirstOrDefault(x => x is CompProperties_ApplicableDesignation && (x as CompProperties_ApplicableDesignation).designationDef == designationDef) == null)
            {
                return cachedResult = false;
            }

            List<SpecialThingFilterDef> l = (List<SpecialThingFilterDef>)MicroDesignations.LdisallowedFilters.GetValue(recipeDef.fixedIngredientFilter);

            if (l != null)
                for (int i = 0; i < l.Count; i++)
                    if (l[i].Worker.Matches(t))
                        return cachedResult = false;

            return cachedResult = true;
        }

        public void FindBuilding()
        {
            if (!reloadBuildable)
                return;

            bool b = false;
            foreach (var user in recipeDef.AllRecipeUsers)
            {
                b = b || user.IsResearchFinished;
                IEnumerable<Building> l = Map.listerBuildings.AllBuildingsColonistOfDef(user);
                if (l.Count() > 0)
                {
                    cachedBuildable = user;
                    cachedStuff = l.FirstOrDefault().Stuff;
                    cachedResearched = true;
                    return;
                }
            }

            cachedBuildable = null;
            cachedStuff = null;
            cachedResearched = b;
            return;
        }
    }
}
