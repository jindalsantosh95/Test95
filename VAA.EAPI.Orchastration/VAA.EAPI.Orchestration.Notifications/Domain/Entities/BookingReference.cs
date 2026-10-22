using System;

namespace VAA.EAPI.Orchestration.Notifications.Domain.Entities
{
    public class BookingReference
    {
        public string AirlineDesignator { get; set; }
        public string PnrLocator { get; set; }
        public DateTime CreatedDate { get; set; }
       
    }
}