using VAA.EAPI.GraphQL.Domain.Trips.Interface.LoyaltyProgramDetails;
using VAA.EAPI.GraphQL.Domain.Trips.Models.PassengerDetails;

namespace VAA.EAPI.GraphQL.Domain.Trips.Services.LoyaltyProgramDetails
{
    public class LoyaltyRequestValidator : ILoyaltyRequestValidator
    {
        public ValidationResult ValidateRequest(LoyaltyMemberDetailsRequest request)
        {
            if (request == null)
                return ValidationResult.Invalid("Request cannot be null");

            if (string.IsNullOrWhiteSpace(request.TripReservationId))
                return ValidationResult.Invalid("TripReservationId is required");

            if (string.IsNullOrWhiteSpace(request.PassengerId))
                return ValidationResult.Invalid("PassengerId is required");

            if (string.IsNullOrWhiteSpace(request.LoyaltyProgramCode))
                return ValidationResult.Invalid("LoyaltyProgramCode is required");

            return ValidationResult.Valid();
        }
    }
}
