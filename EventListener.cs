using Bindito.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Timberborn.Beavers;
using Timberborn.BuildingsBlocking;
using Timberborn.DwellingSystem;
using Timberborn.GameDistricts;
using Timberborn.Navigation;
using Timberborn.SingletonSystem;
using Timberborn.TimeSystem;
using Timberborn.WorkSystem;

namespace HousingOptimize
{
    public class EventListener : ILoadableSingleton
    {
        private EventBus eventBus;
        private DistrictCenterRegistry centerRegistry;

        [Inject]
        public void InjectDependencies(EventBus inEventBus, DistrictCenterRegistry inRegistry)
        {
            eventBus = inEventBus;
            centerRegistry = inRegistry;
        }

        public void Load()
        {
            eventBus.Register(this);
        }

        [OnEvent]
        public void OnDaytimeStart(DaytimeStartEvent daytimeStarted)
        {
            //Get a list of the districts
            DistrictCenter[] centers = centerRegistry.FinishedDistrictCenters.ToArray();

            //Iterate through districts and attempt to move beavers in each
            foreach (DistrictCenter center in centers)
            {
                //Get all the unpaused workplaces in this district
                List<Workplace> workplaces = new List<Workplace>();
                foreach (Workplace current in center.DistrictBuildingRegistry.GetEnabledBuildingsInstant<Workplace>())
                {
                    //Make sure this workplace isn't paused
                    PausableBuilding pause = current.GetComponentFast<PausableBuilding>();
                    if (pause != null && !pause.Paused)
                    {
                        workplaces.Add(current);
                    }
                }

                //Get all the unpaused dwellings in this district
                List<Dwelling> dwellings = new List<Dwelling>();
                foreach (Dwelling current in center.DistrictBuildingRegistry.GetEnabledBuildingsInstant<Dwelling>())
                {
                    //Make sure this dwelling isn't paused
                    PausableBuilding pause = current.GetComponentFast<PausableBuilding>();
                    if (pause != null && !pause.Paused)
                    {
                        dwellings.Add(current);
                    }
                }

                //Kick all the beavers out of their houses
                foreach (Dwelling dwelling in dwellings)
                {
                    dwelling.UnassignAllDwellers();
                }

                //Iterate through all the workplaces and put their workers back in their houses starting with the closest houses first
                foreach (Workplace workplace in workplaces)
                {
                    //Get the list of distances from this workplace to each house and sort them from closest to farthest
                    Accessible workplaceAccessible = workplace.GetComponentFast<Accessible>();
                    List<DwellingDistanceDatum> distances = new List<DwellingDistanceDatum>();
                    foreach (Dwelling dwelling in dwellings)
                    {
                        float currentDistance = 0;
                        workplaceAccessible.FindRoadPath(dwelling.GetComponentFast<Accessible>().Accesses[0], out currentDistance);
                        distances.Add(new DwellingDistanceDatum(dwelling, currentDistance));
                    }
                    distances.Sort();

                    //Assign each worker to the closest available house
                    foreach (Worker worker in workplace.AssignedWorkers)
                    {
                        //Make sure that this worker is not a bot, since bots cannot be in houses
                        Dweller dweller = worker.GetComponentFast<Dweller>();
                        if (dweller == null)
                        {
                            continue;
                        }

                        //Assign this beaver to the closest valid house
                        AssignDweller(distances, dweller);
                    }
                }

                //Put each of the reamining beavers into houses
                foreach (Beaver beaver in center.DistrictPopulation.Beavers)
                {
                    //Get the Dweller object associated with this beaver
                    Dweller dweller = beaver.GetComponentFast<Dweller>();

                    //Assign this dweller to a house with some space left in it
                    if (dweller != null && !dweller.HasHome)
                    {
                        AssignDweller(dwellings, dweller);
                    }
                }
            }
        }

        private void AssignDweller(List<DwellingDistanceDatum> distances, Dweller dweller)
        {
            //Use adult slots first
            foreach (DwellingDistanceDatum distance in distances)
            {
                if (distance.Dwelling.FreeAdultSlots > 0)
                {
                    distance.Dwelling.AssignDweller(dweller);
                    return;
                }
            }

            //No adult slots left, take a child slot
            foreach (DwellingDistanceDatum distance in distances)
            {
                if (distance.Dwelling.HasFreeSlots)
                {
                    distance.Dwelling.AssignDweller(dweller);
                    return;
                }
            }
        }

        private void AssignDweller(List<Dwelling> dwellings, Dweller dweller)
        {
            //Use adult slots first
            foreach (Dwelling dwelling in dwellings)
            {
                if (dwelling.FreeAdultSlots > 0)
                {
                    dwelling.AssignDweller(dweller);
                    return;
                }
            }

            //No adult slots left, take a child slot
            foreach (Dwelling dwelling in dwellings)
            {
                if (dwelling.HasFreeSlots)
                {
                    dwelling.AssignDweller(dweller);
                    return;
                }
            }
        }
    }
}