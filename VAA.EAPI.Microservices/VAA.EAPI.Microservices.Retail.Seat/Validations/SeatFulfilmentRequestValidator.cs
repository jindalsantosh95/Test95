using CSharpFunctionalExtensions;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Seat.Domain.SeatAvailability;

namespace VAA.EAPI.Microservices.Retail.Seat.Validations
{
    public static class SeatFulfilmentRequestValidator
    {
        public static Result<Response<string>> ValidateRequest(SeatFulfilmentRequest request)
        {
            var response = new Response<string>()
            {
                Status = Status.OK,
                Errors = null
            };

            return response;
        }
    }
}
