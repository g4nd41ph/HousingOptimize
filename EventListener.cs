using Bindito.Core;
using System.Collections.Generic;
using System.IO;
using Timberborn.BaseComponentSystem;
using Timberborn.Beavers;
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

        [Inject]
        public void InjectDependencies(EventBus inEventBus)
        {
            eventBus = inEventBus;
        }

        public void Load()
        {
            eventBus.Register(this);
        }

        [OnEvent]
        public void OnDaytimeStart(DaytimeStartEvent daytimeStarted)
        {
            //Get a list of the districts
            DistrictCenter[] centers = BaseComponent.FindObjectsOfType<DistrictCenter>();

            //Get a list of all dwellings
            Dwelling[] allDwellings = BaseComponent.FindObjectsOfType<Dwelling>();

            //Get a list of all employment places
            Workplace[] allWorkplaces = BaseComponent.FindObjectsOfType<Workplace>();

            //Iterate through districts and attempt to move beavers in each
            foreach (DistrictCenter center in centers)
            {
                //Get a list of this district's dwellings
                List<Dwelling> dwellings = new List<Dwelling>();
                foreach (Dwelling current in allDwellings)
                {
                    DistrictBuilding district = current.GetComponentFast<DistrictBuilding>();
                    if (district != null && district.District != null && district.District.Equals(center))
                    {
                        dwellings.Add(current);
                    }
                }

                //Get a list of all this district's workplaces
                List<Workplace> workplaces = new List<Workplace>();
                foreach (Workplace current in allWorkplaces)
                {
                    DistrictBuilding district = current.GetComponentFast<DistrictBuilding>();
                    if (district != null && district.District != null && district.District.Equals(center))
                    {
                        workplaces.Add(current);
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
                    //Get the entrance position
                    Accessible workplaceAccessible = workplace.GetComponentFast<Accessible>();

                    //Assign each worker to the closest available house
                    foreach (Worker worker in workplace.AssignedWorkers)
                    {
                        //Find the closest house
                        float currentDistance = 0;
                        float minDistance = float.MaxValue;
                        Dwelling closestDwelling = null;
                        foreach (Dwelling dwelling in dwellings)
                        {
                            //Compute distance
                            Accessible dwellingAccessible = dwelling.GetComponentFast<Accessible>();
                            workplaceAccessible.FindRoadPath(dwellingAccessible.Accesses[0], out currentDistance);

                            //Check against current minimum
                            if (currentDistance < minDistance && dwelling.HasFreeSlots)
                            {
                                minDistance = currentDistance;
                                closestDwelling = dwelling;
                            }
                        }

                        //Assign this beaver to the closest valid house
                        Dweller dweller = worker.GetComponentFast<Dweller>();
                        if (closestDwelling != null && dweller != null)
                        {
                            closestDwelling.AssignDweller(dweller);
                        }
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
                        foreach (Dwelling dwelling in dwellings)
                        {
                            if (dwelling.HasFreeSlots)
                            {
                                dwelling.AssignDweller(dweller);
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
}