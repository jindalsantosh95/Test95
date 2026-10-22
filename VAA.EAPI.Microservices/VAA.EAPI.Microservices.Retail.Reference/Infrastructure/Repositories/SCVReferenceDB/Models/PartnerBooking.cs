using System;

namespace VAA.EAPI.Microservices.Retail.Reference.Infrastructure.Repositories.SCVReferenceDB.Models
{
    public class PartnerBooking
    {
        public string CabinClass { get; set; }
        public string BookClass { get; set; }
        public string OriginCountryCode { get; set; }
        public string DestinationCountryCode { get; set; }
        public int DistanceInMiles { get; set; }
    }
}
