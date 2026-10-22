using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Seat.Constants;

namespace VAA.EAPI.Microservices.Retail.Seat.Mapper.SeatAvailability
{
    public static class SeatFulfilmentEventCommandResponseMapper
    {
        public static Response<string> SeatFulfilmentResponseMapper(string corelationId, string status)
        {
            return new Response<string>
            {
                Status = Status.Accepted,
                HeaderParameters = new List<ResponseHeader> { new ResponseHeader { Key = Constants.Application.HeaaderParameterlocation, Value = ConfigReader.Get(Environment.SeatFulfilmentResponseUrl).Replace("correlationid", corelationId) } },
            };
        }
    }
}
