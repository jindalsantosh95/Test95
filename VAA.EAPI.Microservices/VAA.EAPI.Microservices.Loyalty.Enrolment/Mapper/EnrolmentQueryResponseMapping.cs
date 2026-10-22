using CSharpFunctionalExtensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Domain;

namespace VAA.EAPI.Microservices.Loyalty.Enrolment.Mapper
{
    public class EnrolmentQueryResponseMapping
    {
        public static Response<EnrolmentResponse> EnrolmentQueryResponseMapper(ServiceResponse serviceResponse)
        {
            return Json.Deserialize<Result<Response<EnrolmentResponse>>>(serviceResponse.JsonResponseData).Value;
        }
    }
}
