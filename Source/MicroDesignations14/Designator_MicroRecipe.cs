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
        //private static readonly Vector2 TerrainTextureCroppedSize = new Vector2(64f, 64f);
        //private static readonly Vector2 DragPriceDrawOffset = new Vector2(19f, 17f);
        private bool reloadBuildable = false;
        private BuildableDef cachedBuildable = null;
        private ThingDef cachedStuff = null;
        private bool cachedResearched = false;
        private bool cachedResult = false;
        private int cachedTick = 0;

        public Designator_MicroRecipe(RecipeDef recipeDef/*, BuildableDef thingUser*/)
        {
            this.recipeDef = recipeDef;

            defaultLabel = recipeDef.label;
            defaultDesc = recipeDef.description;
            soundDragSustain = SoundDefOf.Designate_DragStandard;
            soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
            soundSucceeded = SoundDefOf.Designate_Claim;

            designationDef = DefDatabase<DesignationDef>.AllDefsListForReading.FirstOrDefault(x => x.defName == recipeDef.defName + "Designation");

            if (designationDef?.HasModExtension<DesignatorHotKey>() == true)
                this.hotKey = designationDef.GetModExtension<DesignatorHotKey>().hotKey;

            Order = 200f;
            icon = ContentFinder<Texture2D>.Get("UI/Empty", true);
        }

        public override Command_Action init_Command_Action(Thing t)
        {
            FindBuilding();
            
            if (cachedBuildable == null)
            {
                //Log.Message($"action init empty");
                return null;
            }

            AcceptanceReport acceptanceReport = CanDesignateThing(t);
            if (acceptanceReport.Accepted || (showReverseDesignatorDisabledReason && !acceptanceReport.Reason.NullOrEmpty()))
            {
                BuildableCommand_Action action = new BuildableCommand_Action()
                {
                    buildableDef = cachedBuildable,
                    thingDef = cachedStuff,
                    defaultLabel = LabelCapReverseDesignating(t),
                    icon = IconReverseDesignating(t, out iconAngle, out iconOffset),
                    iconAngle = iconAngle,
                    iconOffset = iconOffset,
                    defaultDesc = (acceptanceReport.Reason.NullOrEmpty() ? DescReverseDesignating(t) : acceptanceReport.Reason),
                    Order = -20f,
                    disabled = !acceptanceReport.Accepted,
                    action = delegate ()
                    {
                        if (!TutorSystem.AllowAction(TutorTagDesignate))
                        {
                            return;
                        }
                        DesignateThing(t);
                        Finalize(true);
                    },
                    hotKey = hotKey,
                    groupKeyIgnoreContent =groupKeyIgnoreContent,
                    groupKey = groupKey
                    
                };
                //Log.Message($"action init {action.buildableDef}, {action.thingDef}");
                return action;
            }
            return null;
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
            Map.designationManager.RemoveAllDesignationsOn(t);
            Map.designationManager.AddDesignation(new Designation(t, Designation));
        }

        bool allowed(Thing thing)
        {
            List<SpecialThingFilterDef> l = (List<SpecialThingFilterDef>)MicroDesignations.LdisallowedFilters.GetValue(recipeDef.fixedIngredientFilter);

            if (l != null)
                for (int i = 0; i < l.Count; i++)
                    if (l[i].Worker.Matches(thing))
                        return false;
            return true;
        }

        public override AcceptanceReport CanDesignateThing(Thing t)
        {
            ThingWithComps thing;
            if (!t.Spawned || (thing = t as ThingWithComps) == null) return false;
            //
            ApplicableDesignationThingComp comp = thing.AllComps?.FirstOrDefault(x => x is ApplicableDesignationThingComp && (x as ApplicableDesignationThingComp).Props.designationDef == designationDef) as ApplicableDesignationThingComp;
            if (comp == null || Map.designationManager.DesignationOn(t, Designation) != null)
                return false;

            if (comp.Allowed == null) comp.Allowed = allowed(t);
            if (comp.Allowed == false)
                return false;

            if (cachedTick == Settings.lastSelectTick)
                return cachedResult;

            cachedTick = Settings.lastSelectTick;

            if (Settings.hide_unresearched && !recipeDef.AvailableNow)
                return cachedResult = false;

            reloadBuildable = true;
            FindBuilding();

            if (Settings.hide_empty || Settings.hide_inactive)
            {
                if (Settings.hide_empty && cachedBuildable == null || Settings.hide_inactive && !cachedResearched)
                    return cachedResult = false;
            }

            return cachedResult = true;
        }

        public void FindBuilding()
        {
            if (!reloadBuildable)
                return;

            reloadBuildable = false;

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
                    //Log.Message($"cached {cachedBuildable}, {cachedStuff}, {cachedResearched}");
                    return;
                }
            }

            cachedBuildable = null;
            cachedStuff = null;
            cachedResearched = b;
            //Log.Message("cached nothing");
            return;
        }
    }
}
