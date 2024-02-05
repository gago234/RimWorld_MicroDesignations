using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using RimWorld;
using System.Reflection;
using System.Linq;
using System.Text;
using MicroDesignations.ModCompatability;
using Unity.Jobs;


namespace MicroDesignations
{
    public class WorkGiver_MicroRecipe : WorkGiver_Scanner
    { 
        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            MicroWorkGiverDef mdef = def as MicroWorkGiverDef;
            if (mdef == null)
                yield break;

            foreach (Designation des in pawn.Map.designationManager.SpawnedDesignationsOfDef(mdef.designationDef))
            {
                if (!des.target.HasThing)
                {
                    Log.ErrorOnce("MicroRecipe designation has no target.", 63126);
                }
                else
                {
                    yield return des.target.Thing;
                }
            }
            yield break;
        }

        public override PathEndMode PathEndMode
        {
            get
            {
                return PathEndMode.ClosestTouch;
            }
        }

        public override Danger MaxPathDanger(Pawn pawn)
        {
            return Danger.Deadly;
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {          
            MicroWorkGiverDef mdef = def as MicroWorkGiverDef;
            if (mdef == null)
                return false;

            if (t.Map.designationManager.DesignationOn(t, mdef.designationDef) == null)
            {
                return false;
            }

            if (pawn.CanReserve(t, 1, -1, null, forced))
            {
                Thing building = null;
                foreach (var user in mdef.recipeDef.AllRecipeUsers)
                {
                    building = t.Map.listerBuildings.AllBuildingsColonistOfDef(user).Where(
                    x => !x.IsForbidden(pawn) &&
                    pawn.CanReserve(x, 1, -1, null, forced) &&
                    (x as IBillGiver) != null &&
                    (x as IBillGiver).CurrentlyUsableForBills() &&
                    pawn.CanReach(x, PathEndMode.InteractionCell, Danger.Deadly)).FirstOrDefault();
                    if (building != null)
                        break;                   
                }

                if (AncientsUtility.HasAncientsExtension(mdef.recipeDef))
                {
                    if(!AncientsUtility.IsUsable(t, mdef.recipeDef))
                        return false;
                    if (TryFindAncientsIngredients(mdef.recipeDef.MakeNewBill(), pawn,t, building) == null)
                        return false;
                }

                if (building != null)
                 return true;
            }

            return false;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {          
            MicroWorkGiverDef mdef = def as MicroWorkGiverDef;
            if (mdef == null)
                return null;

            LocalTargetInfo target = t;
            if (!pawn.CanReserve(target, 1, -1, null, forced) || t.IsBurning() || t.IsForbidden(pawn))
                return null;

            List<Building> list = new List<Building>();
            foreach (var user in mdef.recipeDef.AllRecipeUsers)
            {
                List<Building> buildings = t.Map.listerBuildings.AllBuildingsColonistOfDef(user).Where(
                    x => !x.IsForbidden(pawn) && 
                    pawn.CanReserve(x, 1, -1, null, forced) &&
                    (x as IBillGiver) != null && 
                    (x as IBillGiver).CurrentlyUsableForBills() &&
                    pawn.CanReach(x, PathEndMode.InteractionCell, Danger.Deadly)).ToList();
                list.AddRange(buildings);
            }

            if (list.NullOrEmpty())
                return null;

            list.SortBy(x => x.TryGetComp<CompRefuelable>() == null || x.TryGetComp<CompRefuelable>().HasFuel ? 0f : 99999f + x.Position.DistanceTo(t.Position));

            Thing building = null;
            foreach (var l in list)
            {
                CompRefuelable compRefuelable = l.TryGetComp<CompRefuelable>();
                if (compRefuelable != null && !compRefuelable.HasFuel)
                {
                    if (RefuelWorkGiverUtility.CanRefuel(pawn, l, forced))
                        return RefuelWorkGiverUtility.RefuelJob(pawn, l, forced, null, null);
                }
                else
                {
                    building = l;
                    break;
                }
            }

            Job job = WorkGiverUtility.HaulStuffOffBillGiverJob(pawn, building as IBillGiver, null);
            if (job != null)
            {
                return job;
            }

            job = new Job(mdef.jobDef, building);
            job.targetQueueB = new List<LocalTargetInfo>(1);
            job.countQueue = new List<int>(1);
            job.targetQueueB.Add(t);
            job.countQueue.Add(1);
            job.haulMode = HaulMode.ToCellNonStorage;
            Bill bill = mdef.recipeDef.MakeNewBill();
            Bill_Production bp = bill as Bill_Production;
            if (bp != null)
            {
                bp.repeatCount++;
                bp.SetStoreMode(BillStoreModeDefOf.DropOnFloor);
            }
            job.bill = bill;

            if (AncientsUtility.HasAncientsExtension(mdef.recipeDef))
            {
                return TryFindAncientsIngredients(bill, pawn,t, building); 
            }

            return job;
        }
        private Job TryFindAncientsIngredients(Bill bill, Pawn pawn, Thing t, Thing giver)
        {
            List<ThingCount> chosen = new List<ThingCount>();         
            List<IngredientCount> missingIng = null;
            bool floatMenu = FloatMenuMakerMap.makingFor == pawn;
            if (floatMenu)           
                missingIng = new List<IngredientCount>();             
            if (!WorkGiver_DoBill.TryFindBestBillIngredients(bill, pawn, giver, chosen, missingIng))
            {
                if (FloatMenuMakerMap.makingFor != pawn)
                {
                    bill.nextTickToSearchForIngredients = Find.TickManager.TicksGame + new IntRange(500, 600).RandomInRange;
                }
                else if (floatMenu)
                {
                    string text = missingIng.Select((IngredientCount missing) => missing.Summary).ToCommaList();
                    JobFailReason.Is("MissingMaterials".Translate(text), bill.Label);
                }
                return null;
            }
            if(chosen.Count() > 0)
                chosen.RemoveAll(x => x.Thing.def == t.def);
            chosen.Add(new ThingCount(t, 1));
            Job job = WorkGiver_DoBill.TryStartNewDoBillJob(pawn, bill, giver as IBillGiver, chosen, out _);
            return job;
        }
    }
}
