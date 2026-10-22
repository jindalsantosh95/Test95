using System;
using VAA.EAPI.Microservices.Retail.Reference.Infrastructure.Repositories.SCVReferenceDB.Models;

namespace VAA.EAPI.Microservices.Retail.Reference.Domain
{
    public class BookingDetails
    {
        public string CabinClass { get; set; }
        public string BookClass { get; set; }
        public string OriginCountryCode { get; set; }
        public string DestinationCountryCode { get; set; }
        public int DistanceInMiles { get; set; }

        public static explicit operator BookingDetails(PartnerBooking partnerBooking) => new()
        {

            CabinClass = partnerBooking.CabinClass,
            BookClass = partnerBooking.BookClass,
            DestinationCountryCode = partnerBooking.DestinationCountryCode,
            DistanceInMiles = partnerBooking.DistanceInMiles,
            OriginCountryCode = partnerBooking.OriginCountryCode
        };
    }


}
