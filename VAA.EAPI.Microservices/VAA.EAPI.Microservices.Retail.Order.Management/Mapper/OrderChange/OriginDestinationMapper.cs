using System.Collections.Generic;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain.OrderChange;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Mapper.OrderChange
{
    public class OriginDestinationMapper
    {
        public static List<OriginDestination> Air4ToOrderChangeOnDMapper(OrderChangeRequestData request)
        {
            List<OriginDestination> originDestinations = new();

            foreach(var od in request.OriginDestinations)
            {
                OriginDestination originDestination = new()
                {
                    OriginDestId = od.OriginDestId,
                    OriginAirportCode = od.OriginAirportCode,
                    DestinationAirportCode = od.DestinationAirportCode
                };
                originDestinations.Add(originDestination);
            }
            
            return originDestinations;
        }
    }
}
