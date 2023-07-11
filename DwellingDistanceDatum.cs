using System;
using Timberborn.DwellingSystem;

namespace HousingOptimize
{
    public class DwellingDistanceDatum : IComparable
    {
        //Members
        private Dwelling dwelling;
        private float distance;

        //Constructor
        public DwellingDistanceDatum(Dwelling inDwelling, float inDistance)
        {
            dwelling = inDwelling;
            distance = inDistance;
        }

        //CompareTo
        public int CompareTo(Object other)
        {
            try
            {
                //Cast
                DwellingDistanceDatum comparison = (DwellingDistanceDatum)other;

                //Do comparison basd on distance
                return this.Distance.CompareTo(comparison.Distance);
            }
            catch (Exception ex)
            {
                return -1;
            }
        }

        //Properties
        public Dwelling Dwelling
        {
            get
            {
                return dwelling;
            }
        }

        public float Distance
        {
            get
            {
                return distance;
            }
        }
    }
}
