using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Retail.Reference.Infrastructure.Repositories.SCVReferenceDB.Models
{
    public class Distance
    {
        public string OriginCountryCode { get; set; }
        public string DestinationCountryCode { get; set; }
        public int DistanceInMiles { get; set; }

        public static explicit operator Distance(PartnerBooking partnerBooking) => new()
        {

            OriginCountryCode = partnerBooking.CabinClass,
            DestinationCountryCode = partnerBooking.BookClass,
            DistanceInMiles = partnerBooking.DistanceInMiles
        };

        public static explicit operator Distance(PartnerBookingV2 partnerBooking) => new()
        {

            OriginCountryCode = partnerBooking.CabinClass,
            DestinationCountryCode = partnerBooking.BookClass,
            DistanceInMiles = partnerBooking.DistanceInMiles
        };
    }
}
