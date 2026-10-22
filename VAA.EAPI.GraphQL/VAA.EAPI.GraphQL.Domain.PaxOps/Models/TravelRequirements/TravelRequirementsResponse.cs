using HotChocolate;
using VAA.EAPI.GraphQL.Domain.PaxOps.Helper;
using VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.TravelRequirements;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Models.TravelRequirements
{
    public class TravelRequirementsResponse
    {
        [GraphQLDescription("Opaque token used for later travel-requirements actions. Do not parse or log.")]
        public string TravelRequirementsId { get; set; }

        [GraphQLDescription("Booking-level reference details.")]
        public Reference Reference { get; set; }

        [GraphQLDescription("Passengers returned for the booking, including documents and per-slice requirement state.")]
        public IEnumerable<Passenger> Passengers { get; set; }

        [GraphQLDescription("Booking itinerary slices. Use sliceId to correlate with each passenger's sliceRequirements.")]
        public IEnumerable<Slice> Slices { get; set; }

        [GraphQLDescription("Whether the same emergency contact can be applied to every passenger.")]
        public bool ApplyEmergencyContactToAllPax { get; set; }

        public static async Task<TravelRequirementsResponse> FromServiceResponse(TravelRequirementsServiceResponse response)
        {
            if (response == null)
            {
                throw new System.InvalidOperationException("Travel requirements service returned a null response.");
            }

            return new TravelRequirementsResponse
            {
                TravelRequirementsId = await TravelRequirementsIdHelper.CreateTravelRequirementsId(null, response.RequirementsId, true, response.Passengers?.FirstOrDefault()?.GivenNames, response.Passengers?.FirstOrDefault()?.Surname, response?.Reference?.PnrLocator, response.Passengers?.Select(psg => psg.PassengerId)),
                Reference = response.Reference != null ? (Reference)response.Reference : null,
                Passengers = response.Passengers?.Select(p => (Passenger)p),
                Slices = response.Slices?.Select(s => (Slice)s),
                ApplyEmergencyContactToAllPax = response.ApplyEmergencyContactToAllPax
            };
        }
    }
}
