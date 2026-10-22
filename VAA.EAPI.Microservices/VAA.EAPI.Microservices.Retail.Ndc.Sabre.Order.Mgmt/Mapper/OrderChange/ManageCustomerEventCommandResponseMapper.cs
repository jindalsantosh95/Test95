using System.Collections.Generic;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Mapper.OrderChange
{
    public class ManageCustomerEventCommandResponseMapper
    {
        public static Response<string> ManageCustomerResponseMapper(string corelationid)
        {
            return new Response<string>
            {
                Status = Status.Accepted,
                HeaderParameters = new List<ResponseHeader> { new ResponseHeader { Key = Constants.Application.HeaaderParameterlocation, Value = ConfigReader.Get(Constants.Environment.ManageCustomerResponseUrl).Replace("{correlationid}", corelationid) } },
            };
        }
    }
}
