using System;
using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Order.Constants;

namespace VAA.EAPI.Microservices.Retail.Order.Mapper.InflightOrders
{
    class OrderAssociationEventCommandResponseMapper
    {
        public static Response<string> OrderAssociationResponseMapper(string correlationid)
        {
            return new Response<string>
            {
                Status = Status.Accepted,
                HeaderParameters = new List<ResponseHeader> { new ResponseHeader { Key = Constants.Application.HeaaderParameterlocation, Value = ConfigReader.Get(Constants.Environment.OrderAssociationResponseUrl).Replace("{correlationid}", correlationid) } },
            };
        }
    }
}
