using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.Domain.InflightOrders.CommonReferenceDetails.Interface
{
    public interface ICachedAirport
    {
        List<Models.AirportDetails> GetCachedAirport();
    }
}
