using PassengerDetailsMsSchema = VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.PassengerDetails;

namespace VAA.EAPI.GraphQL.Domain.Trips.Interface
{
    public interface IPassengerDetailsProvider
    {
        Task<PassengerDetailsMsSchema.PassengerDetails> GetPassengerDetails(string tripReservationId);
        Task<PassengerDetailsMsSchema.PassengerDetails> ManagePassengerLoyaltyMemberDetails(PassengerDetailsMsSchema.PassengerDetails passengerDetails);
    }
}
