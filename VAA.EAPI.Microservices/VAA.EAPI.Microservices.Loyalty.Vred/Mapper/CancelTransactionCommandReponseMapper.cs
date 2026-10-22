using System.Collections.Generic;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Mapper
{
    public class CancelTransactionCommandReponseMapper
    {
        public static Response<string> CancelTransactionResponseMapper(string corelationid, string status)
        {
            return new Response<string>
            {
                Status = Status.Accepted,
                HeaderParameters = new List<ResponseHeader> { new ResponseHeader { Key = Constants.Application.HeaderParameterlocation, Value = ConfigReader.Get(Constants.Environment.CancelTransactionResponseUrl).Replace("{correlationid}", corelationid) } },
            };
        }
    }
}
