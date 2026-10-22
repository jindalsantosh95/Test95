using System;
using System.Security.Policy;

namespace VAA.EAPI.Orchestration.Notifications.Infrastructure.Models.NotificationMs
{
    public class BookingReference
    {
        public static explicit operator BookingReference(Domain.Entities.BookingReference source) => new()
        {
            PnrLocator = source.PnrLocator,
            CreatedDate = source.CreatedDate,
            AirlineDesignator = source.AirlineDesignator
        };

        public string AirlineDesignator { get; set; }
        public string PnrLocator { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}