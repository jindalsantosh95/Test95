using System.Collections.Generic;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Mapper
{
    public class CreateTransactionResponseMapper
    {
        public static Response<string> BuildTransactionResponse(string corelationid)
        {
            return new Response<string>
            {
                Status = Status.Accepted,
                HeaderParameters = new List<ResponseHeader> { new ResponseHeader { Key = Constants.Application.HeaaderParameterlocation, Value = ConfigReader.Get(Constants.Environment.CreateTransactionResponseUrl).Replace("{correlationid}", corelationid) } },
            };
        }
    }
}