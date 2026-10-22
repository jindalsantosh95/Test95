using System.Collections.Generic;

namespace VAA.EAPI.Domain.Virgin.Models.Products.Brand
{
    public class FlightLeg
    {
        public OperatingCarrier OperatingCarrier { get; set; }
        public Aircraft Aircraft { get; set; }
        public string OriginAirportCode { get; set; }
        public string DestAirportCode { get; set; }
        public string SchedDepartLocalTs { get; set; }
        public string SchedDepartUtcTs { get; set; }
        public string SchedArrivalLocalTs { get; set; }
        public string SchedArrivalUtcTs { get; set; }
        public List<BrandProduct> BrandProduct { get; set; }
    }
}
