using CSharpFunctionalExtensions;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.CustomerDetails;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.Enrolment.FCNumberGeneration.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.Enrolment.FCNumberGeneration.Mapper
{
    public static class FCNumGenResponseMapper
    {
        public static Result<Response<EnrolmentResponse>> MapFCGENAPIResponseToEAPIResponse(FCNumberGenResponse fCNumberGenResponse)
        {
            Response<EnrolmentResponse> response = new Response<EnrolmentResponse>();
            response.Value = new EnrolmentResponse
            {
                Memberdetails = new MemberStatus()
                {
                    FlyingClubCardNumber = fCNumberGenResponse?.ResponseMessage,
                    MemberNumber = fCNumberGenResponse?.ResponseMessage,
                }
            };
            return response;

        }
    }
}
