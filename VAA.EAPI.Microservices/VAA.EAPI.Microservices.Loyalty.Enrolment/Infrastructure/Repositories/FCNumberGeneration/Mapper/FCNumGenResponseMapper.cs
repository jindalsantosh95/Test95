using CSharpFunctionalExtensions;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Domain;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Infrastructure.Repositories.FCNumberGeneration.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Enrolment.Infrastructure.Repositories.FCNumberGeneration.Mapper
{
    public static class FCNumGenResponseMapper
    {
        public static Result<Response<EnrolmentResponse>> MapFCGENAPIResponseToEAPIResponse(FCNumberGenResponse fCNumberGenResponse)
        {
            Response<EnrolmentResponse> response = new Response<EnrolmentResponse>();
            response.Value = new EnrolmentResponse
            {
                Memberdetails = new Domain.CustomerDetails.MemberStatus()
                {
                    FlyingClubCardNumber = fCNumberGenResponse?.ResponseMessage,
                    MemberNumber = fCNumberGenResponse?.ResponseMessage,
                }
            };
            return response;

        }
    }
}
