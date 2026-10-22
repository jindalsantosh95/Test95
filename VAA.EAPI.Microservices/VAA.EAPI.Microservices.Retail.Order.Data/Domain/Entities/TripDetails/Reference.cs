using System;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.TripDetails
{
    public class Reference
    {
        public string? AirlineDesignator { get; set; }
        public string? PnrLocator { get; set; }
        public string? CreatedDate { get; set; }
        public string? ConfirmationNumber { get; set; }
        public string? ChainCode { get; set; }
        public DateTime? BookingDate { get; set; }
    }
}