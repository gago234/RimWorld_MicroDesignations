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

        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            MicroWorkGiverDef mdef = def as MicroWorkGiverDef;
            if (mdef == null)
                return true;
                   
            return !pawn.Map.designationManager.AnySpawnedDesignationOfDef(mdef.designationDef);            
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

            if (t.def.hasInteractionCell && !pawn.CanReserveSittableOrSpot(t.InteractionCell, forced))
                return false;

            if (!pawn.CanReserve(t, 1, -1, null, forced))
            {
                return false;
            }

            Thing building = null;
            foreach (var user in mdef.recipeDef.AllRecipeUsers)
            {
                foreach (var x in t.Map.listerBuildings.AllBuildingsColonistOfDef(user))
                {
                    if (!pawn.CanReserve(x, 1, -1, null, forced) || (x.def.hasInteractionCell && !pawn.CanReserveSittableOrSpot(x.InteractionCell, forced)))
                        continue;           
                  
                    if (!x.IsForbidden(pawn) && !t.IsBurning() && !t.IsBrokenDown() && x as IBillGiver != null && 
                        (x as IBillGiver).CurrentlyUsableForBills() &&
                        pawn.CanReach(x, PathEndMode.InteractionCell, Danger.Deadly))
                    {
                        building = x;
                        break;
                    }
                }
                if (building != null)
                    break;                                  
            }

            if (building != null)
            {
                if (AncientsUtility.HasAncientsExtension(mdef.recipeDef))
                {
                    if(!AncientsUtility.IsUsable(t, mdef.recipeDef))
                        return false;
                    if (TryFindAncientsIngredients(mdef.recipeDef.MakeNewBill(), pawn,t, building) == null)
                        return false;
                }
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
            if (!pawn.CanReserve(target, 1, -1, null, forced) || t.IsBurning() || t.IsForbidden(pawn) || t.IsBrokenDown())
                return null;

            HashSet<Building> buildings = new HashSet<Building>();
            foreach (var user in mdef.recipeDef.AllRecipeUsers)
            {               
                foreach(var x in t.Map.listerBuildings.AllBuildingsColonistOfDef(user))
                {
                    if (!pawn.CanReserve(x, 1, -1, null, forced) || (x.def.hasInteractionCell && !pawn.CanReserveSittableOrSpot(x.InteractionCell, forced)))
                        continue;

                    if(!x.IsForbidden(pawn) && 
                        (x as IBillGiver) != null && (x as IBillGiver).CurrentlyUsableForBills() 
                        && x.def.hasInteractionCell && pawn.CanReach(x, PathEndMode.InteractionCell, Danger.Deadly))
                    {                        
                        buildings.Add(x);
                    }
                }
            }

            if (buildings.EnumerableNullOrEmpty())
                return null;

            buildings.OrderBy(x => x.TryGetComp<CompRefuelable>() == null || x.TryGetComp<CompRefuelable>().HasFuel ? 0f : 99999f + x.Position.DistanceTo(t.Position));

            Thing building = null;
            foreach (var b in buildings)
            {
                CompRefuelable compRefuelable = b.TryGetComp<CompRefuelable>();
                if (compRefuelable != null && !compRefuelable.HasFuel)
                {
                    if (RefuelWorkGiverUtility.CanRefuel(pawn, b, forced))
                        return RefuelWorkGiverUtility.RefuelJob(pawn, b, forced, null, null);
                }
                else
                {
                    building = b;
                    break;
                }
            }

            Job job = WorkGiverUtility.HaulStuffOffBillGiverJob(pawn, building as IBillGiver, null);
            if (job != null)
            {
                return job;
            }

            job = JobMaker.MakeJob(mdef.jobDef, building);
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
            {
                chosen.RemoveAll(x => x.Thing.def == t.def);
                chosen.Add(new ThingCount(t, 1));               
            }
            Job job = WorkGiver_DoBill.TryStartNewDoBillJob(pawn, bill, giver as IBillGiver, chosen, out _);
            return job;
        }
    }
}
