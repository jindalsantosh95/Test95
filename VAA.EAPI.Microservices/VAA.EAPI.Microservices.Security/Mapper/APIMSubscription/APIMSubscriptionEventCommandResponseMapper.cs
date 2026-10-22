using System.Collections.Generic;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.Microservices.Security.Mapper.APIMSubscription
{
    public class APIMSubscriptionEventCommandResponseMapper
    {
        public static Response<string> APIMSubscriptionEventResponse(string correlationId, Status status, string responseUrl)
        {
            return new Response<string>
            {
                Status = status,
                HeaderParameters = new List<ResponseHeader> { new ResponseHeader { Key = Constants.Application.HeaderParameterLocation, Value = ConfigReader.Get(responseUrl).Replace("{correlationid}", correlationId) } }
            };
        }

    }
}