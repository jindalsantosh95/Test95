using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Order.Create.Constants;

namespace VAA.EAPI.Microservices.Retail.Order.Create.Mapper
{
    public class OrderCreateResponseMapper
    {
        public static Response<string> ResponseMapper(string correlationid, string status)
        {
            return new Response<string>
            {
                Status = Status.Accepted,
                HeaderParameters = new List<ResponseHeader> { new ResponseHeader { Key = Constants.Application.HeaderParameterlocation, Value = ConfigReader.Get(Environment.OrderCreateResponseUrl).Replace("{correlationid}", correlationid) } },
            };
        }
    }
}
