using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.TripDetails
{
    public class TripDetailsRequest
    {
        public IEnumerable<TripDetails> TripDetails { get; set; }
    }
}