using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Order.Management.Constants;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Mapper.OrderChange
{
    public class ManageSpecialServiceCommandResponseMapper
    {
        public static Response<string> ManageSpecialServiceResponseMapper(string corelationid)
        {
            return new Response<string>
            {
                Status = Status.Accepted,
                HeaderParameters = new List<ResponseHeader> { new ResponseHeader { Key = Constants.Application.HeaaderParameterlocation, Value = ConfigReader.Get(Environment.ManageSpecialServiceResponseUrl).Replace("{correlationid}", corelationid) } },
            };
        }
    }
}
