using System;
using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.Microservices.Loyalty.Enrolment.Mapper
{
    public class EnrolmentResponseMapper
    {
        public static Response<string> ResponseMapper(string correlationid, string status)
        {
            return new Response<string>
            {
                Status = Status.Accepted,
                HeaderParameters = new List<ResponseHeader> { new ResponseHeader { Key = Constants.Application.HeaderParameterlocation, Value = Environment.GetEnvironmentVariable(Constants.Environment.EnrolmentResponseUrl).Replace("{correlationid}", correlationid) } },
            };
        }
    }
}
