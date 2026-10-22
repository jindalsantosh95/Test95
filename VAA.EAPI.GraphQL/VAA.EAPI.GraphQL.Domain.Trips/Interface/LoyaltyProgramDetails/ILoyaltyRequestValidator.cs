using VAA.EAPI.GraphQL.Domain.Trips.Models.PassengerDetails;

namespace VAA.EAPI.GraphQL.Domain.Trips.Interface.LoyaltyProgramDetails
{
    public interface ILoyaltyRequestValidator
    {
        ValidationResult ValidateRequest(LoyaltyMemberDetailsRequest request);
    }
}
