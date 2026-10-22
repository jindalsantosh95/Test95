using System.Collections.Generic;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Mapper.CPD
{
    public class CreatePaymentDocCommandResponseMapper
    {
        public static Response<string> CreatePaymentDocumentResponseMapper(string corelationid)
        {
            return new Response<string>
            {
                Status = Status.Accepted,
                HeaderParameters = new List<ResponseHeader> { new ResponseHeader { Key = Constants.Application.HeaderParameterlocation, Value = ConfigReader.Get(Constants.Environment.CreatePaymentDocumentResponseUrl).Replace("{correlationid}", corelationid) } },
            };
        }
    }
}
