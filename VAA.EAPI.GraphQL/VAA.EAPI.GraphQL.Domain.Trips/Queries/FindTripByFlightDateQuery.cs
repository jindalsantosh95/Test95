using HotChocolate;
using HotChocolate.Types;
using VAA.EAPI.GraphQL.Domain.Trips.Interface;
using VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin;

namespace VAA.EAPI.GraphQL.Domain.Trips.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public sealed class FindTripByFlightDateQuery(IScvBookingProvider provider)
    {
        private readonly IScvBookingProvider _provider = provider ?? throw new ArgumentNullException(nameof(provider));

        [GraphQLName("FindTripByFlightDate")]
        [GraphQLDescription("Returns SCV bookings matching a PNR, scheduled local flight departure date and passenger name using the TripDetails booking contract. Payment details are excluded. No match or multiple matches return an empty list.")]
        public Task<IReadOnlyList<Booking>> LookupAsync(string pnr, string flightDate, string givenName, string surname, CancellationToken cancellationToken = default)
            => _provider.LookupAsync(pnr, flightDate, givenName, surname, cancellationToken);
    }
}
