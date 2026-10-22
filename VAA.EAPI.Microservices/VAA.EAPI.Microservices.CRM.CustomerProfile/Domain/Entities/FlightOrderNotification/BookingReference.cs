using System;

namespace VAA.EAPI.Microservices.CRM.CustomerProfile.Domain.Entities.FlightOrderNotification
{
    public class BookingReference
    {
        public string AirlineDesignator { get; set; }
        public string PnrLocator { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}