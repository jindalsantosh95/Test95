using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Flight.Services.Domain;

namespace VAA.EAPI.Microservices.Flight.Services.Mapper
{
    public class FlightServiceResponseToRedisCacheMapper
    {
        public static FlightServiceResponseDocument RedisDocumentMapper(FlightServiceResponse flightServiceResponse, string docId, string docType)
        {
            return new FlightServiceResponseDocument
            {
                Id = docId,
                PartitionKey = docId,
                CreatedBy = Constants.Application.NdcChannel,
                CreatedOn = DateTime.UtcNow.ToString(Constants.Application.IsoDateTimeFormat),
                DocType = docType,
                Pos = flightServiceResponse.Pos,
                Parties = flightServiceResponse.Parties,
                OrderId = flightServiceResponse.OrderId,
                Offers = flightServiceResponse.Offers
            };
        }
    }
}
