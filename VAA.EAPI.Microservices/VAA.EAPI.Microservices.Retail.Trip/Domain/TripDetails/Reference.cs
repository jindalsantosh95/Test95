using System;
using VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaMyTrips;

namespace VAA.EAPI.Microservices.Retail.Trip.Domain.TripDetails
{
    public class Reference
    {
        public string? AirlineDesignator { get; set; }
        public string? PnrLocator { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? ConfirmationNumber { get; set; }
        public string? ChainCode { get; set; }
        public DateTime? BookingDate { get; set; }

        public static explicit operator Reference(Reservation source)
        {
            var firstRef = source?.BookingReferences?.FirstOrDefault(x => x?.Organization?.Id == "VS");

            return new Reference
            {
                AirlineDesignator = firstRef?.Organization?.Id,
                PnrLocator = firstRef?.Id,
                CreatedDate = source?.BookingCreationDt
            };
        }
    }
}
