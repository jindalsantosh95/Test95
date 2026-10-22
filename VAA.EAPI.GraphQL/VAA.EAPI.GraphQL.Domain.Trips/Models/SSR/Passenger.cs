using HotChocolate;
using VAA.EAPI.GraphQL.Domain.Trips.Models.Enum;
using VAA.EAPI.GraphQL.Domain.Trips.Models.ReferenceData;
using SSRMS = VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.SSRMS;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.SSR
{
    [GraphQLName("SSRPassenger")]
    public class Passenger
    {
        public string? PassengerResourceId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public IEnumerable<PassengerSegment>? PassengerSegments { get; set; }

        public static explicit operator Passenger((SSRMS.Passenger passenger, PassengerConversionType type, IEnumerable<SSRMealRefData> ssrMealDetailsRefData, IEnumerable<AirportRefData> airports) input)
        {
            var (passenger, type, ssrMealDetailsRefData, airports) = input;
            if (passenger == null)
                return null;

            return new()
            {
                PassengerResourceId = passenger.PassengerResourceId,
                FirstName = passenger.FirstName,
                LastName = passenger.LastName,
                PassengerSegments = passenger.SsrSegments.Select(ss => (PassengerSegment)(ss, input.type, ssrMealDetailsRefData, airports))
            };
        }
    }
}

