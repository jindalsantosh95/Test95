using System;
using System.Collections.Generic;

namespace VAA.EAPI.Web.Search.Models.Brand.Response
{
    public class Segment
    {
        public DateTime FlightDate { get; set; }
        public string Carrier { get; set; }
        public string CarrierName { get; set; }
        public string FareFamilyType { get; set; }
        public string BookingClass { get; set; }
        public string CabinName { get; set; }
        public int FlightNumber { get; set; }
        public string Origin { get; set; }
        public string Destination { get; set; }
        public string Product { get; set; }
        public Amenities Amenities { get; set; }
        public IEnumerable<BoardingPrority> BoardingPriorities { get; set; }
        public IEnumerable<BaggagePolicy> CarryOnBaggage { get; set; }
        public IEnumerable<BaggagePolicy> CheckedBaggage { get; set; }
        public IEnumerable<CheckInPriority> CheckInPriority { get; set; }
        public IEnumerable<LoungeAccess> LoungeAccess { get; set; }
        public IEnumerable<SeatSelection> SeatSelection { get; set; }
        public IEnumerable<UpgradeEligibility> UpgradeEligibility { get; set; }
        public IEnumerable<UPA> Upas { get; set; }
        public string OverNight { get; set; }
        public bool DominantLeg { get; set; }
    }
}