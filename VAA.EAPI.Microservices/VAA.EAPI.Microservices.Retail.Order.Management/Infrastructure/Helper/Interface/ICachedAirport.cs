using System.Collections.Generic;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain.CommonRefDB;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Helper.Interface
{
    public interface ICachedAirport
    {
        List<AirportDetails> GetCachedAirport();
    }
}
