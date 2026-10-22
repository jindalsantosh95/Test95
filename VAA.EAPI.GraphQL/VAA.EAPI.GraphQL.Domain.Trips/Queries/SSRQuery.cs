using HotChocolate;
using HotChocolate.Types;
using VAA.EAPI.GraphQL.Domain.Trips.Helper;
using VAA.EAPI.GraphQL.Domain.Trips.Interface;
using VAA.EAPI.GraphQL.Domain.Trips.Models.Enum;
using VAA.EAPI.GraphQL.Domain.Trips.Models.ReferenceData;
using VAA.EAPI.GraphQL.Domain.Trips.Models.SSR;

namespace VAA.EAPI.GraphQL.Domain.Trips.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class SSRQuery
    {
        private ISSRDetailsProvider _ssrMealDetailsQueryProvider;
        private readonly IReferenceServiceProvider _referenceServiceProvider;

        public SSRQuery(ISSRDetailsProvider ssrMealDetailsQueryProvider, IReferenceServiceProvider referenceServiceProvider)
        {
            _ssrMealDetailsQueryProvider = ssrMealDetailsQueryProvider ?? throw new ArgumentNullException(nameof(ssrMealDetailsQueryProvider));
            _referenceServiceProvider = referenceServiceProvider ?? throw new ArgumentNullException(nameof(referenceServiceProvider));
        }

        [GraphQLName("GetSpecialAssistance")]
        [GraphQLDescription("Retrieve special assistance SSR details using tripReservationID")]
        public async Task<PassengerResponse> GetSpecialAssistance(string tripReservationID)
        {
            ArgumentNullException.ThrowIfNull(tripReservationID);

            IEnumerable<AirportRefData> refAirportData = Enumerable.Empty<AirportRefData>();
            IEnumerable<SSRMealRefData> ssrDetails = Enumerable.Empty<SSRMealRefData>();

            var ssrMealDetails = await _ssrMealDetailsQueryProvider.QuerySSRMealDetailsAsync(await BookingIdHelper.GetMSBookingId(tripReservationID));

            Parallel.Invoke(
                      new Action(() => refAirportData = _referenceServiceProvider.RetrieveAirportDataAsync().GetAwaiter().GetResult()),
                      new Action(() => ssrDetails = _referenceServiceProvider.RetrieveSSRMealDataAsync().GetAwaiter().GetResult())
                    );

            return new PassengerResponse
            {
                Passengers = ssrMealDetails?.Passengers?
                .Select(p => (Passenger)(p, PassengerConversionType.SPECIALASSISTANCE, ssrDetails, refAirportData))
                .Where(p => p != null)
            };
        }

        [GraphQLName("GetSpecialMeals")]
        [GraphQLDescription("Retrieve Special meals SSR details using tripReservationID")]
        public async Task<PassengerResponse> GetSpecialMeals(string tripReservationID)
        {
            ArgumentNullException.ThrowIfNull(tripReservationID);

            var ssrMealDetails = await _ssrMealDetailsQueryProvider.QuerySSRMealDetailsAsync(await BookingIdHelper.GetMSBookingId(tripReservationID));
            var mealDetailsRefData = await _referenceServiceProvider.RetrieveSSRMealDataAsync();

            return new PassengerResponse
            {
                Passengers = ssrMealDetails?.Passengers?
                .Select(p => (Passenger)(p, PassengerConversionType.MEAL, mealDetailsRefData, null))
                .Where(p => p != null)
            };
        }
    }
}