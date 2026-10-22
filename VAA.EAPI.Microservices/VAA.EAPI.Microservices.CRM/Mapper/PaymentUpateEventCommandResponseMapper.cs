using System.Collections.Generic;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;

namespace VAA.EAPI.Microservices.CRM.Mapper
{
    public class PaymentUpateEventCommandResponseMapper
    {
        public static Response<string> MapToEventCommandResponse(string corelationid, string status)
        {
            return new Response<string>
            {
                Status = Status.Accepted,
                HeaderParameters = new List<ResponseHeader> { new ResponseHeader { Key = Constants.Application.HeaaderParameterlocation, Value = ConfigReader.Get(Constants.Environment.CRMWebApiPaymentUpdateResponseUrl).Replace("{correlationid}", corelationid) } },
            };
        }
    }
}
