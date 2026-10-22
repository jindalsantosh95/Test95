using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Retail.Trip.Domain.TripDetails
{
    public class Ticket
    {
        public string? Number { get; set; }
        public string? IssueDt { get; set; }
        public string? PosCurrencyCode { get; set; }
        public string? DeliveryType { get; set; }

        public static explicit operator Ticket(Infrastructure.Proxy.DeltaMyTrips.Ticket source)
        {
            return new Ticket
            {
                Number = source?.Number,
                IssueDt = source?.IssueDt,
                PosCurrencyCode = source?.PosCurrencyCode,
                DeliveryType = source?.DeliveryType
            };
        }
    }
}