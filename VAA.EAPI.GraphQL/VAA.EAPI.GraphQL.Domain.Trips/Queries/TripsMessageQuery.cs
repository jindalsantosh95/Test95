using HotChocolate;
using HotChocolate.Types;
using VAA.EAPI.GraphQL.Domain.Trips.Helper;
using VAA.EAPI.GraphQL.Domain.Trips.Interface;
using VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin;


namespace VAA.EAPI.GraphQL.Domain.Trips.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class TripsMessageQuery
    {
        private ITripsDetailsQueryProvider _tripsDetailsQueryProvider;

        public TripsMessageQuery(ITripsDetailsQueryProvider tripsDetailsQueryProvider)
        {
            _tripsDetailsQueryProvider = tripsDetailsQueryProvider ?? throw new ArgumentNullException(nameof(tripsDetailsQueryProvider));
        }

        [GraphQLName("BookingMessages")]
        [GraphQLDescription("Retrieve trip Message details using tripreservation id")]
        public async Task<MessageResponse> GetTripMessageDetails(string tripReservationID)
        {
            if (tripReservationID == null)
            {
                throw new ArgumentNullException(nameof(tripReservationID));
            }

            return await _tripsDetailsQueryProvider.GetTripMessageDetails(tripReservationID);
        }

        [GraphQLName("getFlyReadyStatus")]
        [GraphQLDescription("Retrieve FlyReady status using trip reservation ID, passenger ID, and slice ID")]
        public async Task<IEnumerable<FlyReadyStatus>> GetFlyReadyStatusAsync(string tripReservationID)
        {
            if (string.IsNullOrWhiteSpace(tripReservationID))
            {
                throw new ArgumentNullException(nameof(tripReservationID), "Trip reservation ID cannot be null or empty.");
            }

            var msBookingId = await BookingIdHelper.GetMSBookingId(tripReservationID);
            return await _tripsDetailsQueryProvider.GetFlyreadyTripMessageStatus(msBookingId);
        }
    }
}
