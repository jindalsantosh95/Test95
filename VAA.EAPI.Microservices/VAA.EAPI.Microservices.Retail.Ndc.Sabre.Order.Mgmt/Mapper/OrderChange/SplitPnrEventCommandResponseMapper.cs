using System.Collections.Generic;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Mapper.OrderChange
{
    public static class SplitPnrEventCommandResponseMapper
    {
        public static Response<string> SplitPnrResponseMapper(string corelationid)
        {
            return new Response<string>
            {
                Status = Status.Accepted,
                HeaderParameters = new List<ResponseHeader> { new ResponseHeader { Key = Constants.Application.HeaaderParameterlocation, Value = ConfigReader.Get(Constants.Environment.SplitPnrResponseUrl).Replace("{correlationid}", corelationid) } },
            };
        }
    }
}
