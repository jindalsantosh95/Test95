using System;
using System.Collections.Generic;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.BrandMS
{
    public class Segment
    {
        public DateTime? FlightDate { get; set; }
        public string Carrier { get; set; }
        public int? FlightNumber { get; set; }
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
        public IEnumerable<Upa> Upas { get; set; }
        public string OverNight { get; set; }
    }
}
