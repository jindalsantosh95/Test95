using VAA.EAPI.GraphQL.Domain.Trips.Models.Enum;
using VAA.EAPI.GraphQL.Domain.Trips.Models.ReferenceData;
using SSRMS = VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.SSRMS;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.SSR
{
    public class PassengerResponse
    {
        public IEnumerable<Passenger>? Passengers { get; set; }

        public static explicit operator PassengerResponse((SSRMS.PassengerSSRResponse source, PassengerConversionType type, IEnumerable<SSRMealRefData> ssrMealDetails, IEnumerable<AirportRefData> airports) input)
        {
            if (input.source == null) return null;

            return new PassengerResponse
            {
                Passengers = input.source.Passengers?
                    .Where(p => p != null)
                    .Select(p => (Passenger)(p, input.type, input.ssrMealDetails, input.airports))
            };
        }
    }
}

