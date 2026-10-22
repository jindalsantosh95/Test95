using VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin;

namespace VAA.EAPI.GraphQL.Domain.Trips.Interface
{
    public interface IScvBookingProvider
    {
        Task<IReadOnlyList<Booking>> LookupAsync(string pnr, string flightDate, string givenName, string surname, CancellationToken cancellationToken = default);
    }
}
