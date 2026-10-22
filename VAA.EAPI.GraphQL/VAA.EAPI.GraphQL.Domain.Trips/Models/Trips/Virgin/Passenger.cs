using HotChocolate;
using NodaTime;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.Trips.Models.ReferenceData;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin
{
    [GraphQLName("TripsPassenger")]
    public class Passenger
    {
        public string? CustomerId { get; set; }
        public string? NameNumber { get; set; }
        public string? PassengerId { get; set; }
        public string? AssociatedInfantId { get; set; }
        public string? Id { get; set; }
        public LoyaltyMembership? LoyaltyMembership { get; set; }
        public PassengerType? PassengerType { get; set; }
        public string? Title { get; set; }
        public string? GivenNames { get; set; }
        public string? LastName { get; set; }
        public string? FirstNameNumber { get; set; }
        public string? LastNameNumber { get; set; }
        public string? Gender { get; set; }
        public string? DateOfBirth { get; set; }
        public LocalDate? BirthDate { get; set; }
        public string? Email { get; set; }
        public Phone? Phone { get; set; }
        public IEnumerable<Document>? Documents { get; set; }
        public string? FirstName { get; set; }
        public IEnumerable<Slice>? Slices { get; set; }
        public bool? LeadPassenger { get; set; }

        public static explicit operator Passenger((Infrastructure.Models.TripMS.Passenger passenger, IEnumerable<CabinRefData> cabinDetails) source) =>
            (Passenger)(source.passenger, source.cabinDetails, null);

        public static explicit operator Passenger((Infrastructure.Models.TripMS.Passenger passenger, IEnumerable<CabinRefData> cabinDetails, Infrastructure.Models.CheckInMS.TravelRequirementsResponse travelRequirements) source) =>
            (Passenger)(source.passenger, source.cabinDetails, source.travelRequirements, null);

        public static explicit operator Passenger((Infrastructure.Models.TripMS.Passenger passenger, IEnumerable<CabinRefData> cabinDetails, Infrastructure.Models.CheckInMS.TravelRequirementsResponse travelRequirements, IEnumerable<Infrastructure.Models.TripMS.Slice> bookingSlices) source)
        {
            var (passenger, cabinDetails, travelRequirements, bookingSlices) = source;

            if (passenger == null) return null;

            var passengerRequirements = travelRequirements?.Passengers?.FirstOrDefault(requirementsPassenger =>
                passenger.FirstNameNumber == requirementsPassenger.PassengerFirstNin &&
                passenger.LastNameNumber == requirementsPassenger.PassengerLastNin);

            return new Passenger()
            {
                CustomerId = passenger.CustomerId,
                DateOfBirth = passenger.DateOfBirth,
                Documents = passenger.Documents?.Select(x => (Document)x),
                Email = passenger.Email,
                FirstName = passenger.FirstName,
                LastName = passenger.LastName,
                GivenNames = passenger.GivenNames,
                FirstNameNumber = passenger.FirstNameNumber,
                LastNameNumber = passenger.LastNameNumber,
                Gender = passenger.Gender,
                Id = passenger.Id,
                NameNumber = passenger.NameNumber,
                PassengerId = passenger.PassengerId,
                AssociatedInfantId = passenger.AssociatedInfantId,
                PassengerType = (PassengerType)passenger.PassengerType,
                Title = passenger.Title,
                LoyaltyMembership = (LoyaltyMembership)passenger.LoyaltyMembership,
                Phone = (Phone)passenger.Phone,
                Slices = passenger.Slices?.Where(x => x.IropProtect == null).Select(x =>
                {
                    var bookingSlice = bookingSlices?.FirstOrDefault(slice => slice.Id == x.Id);
                    var travelRequirementsSlice = travelRequirements?.Slices?.FirstOrDefault(requirementsSlice =>
                        requirementsSlice.Origin?.Code.Is(bookingSlice?.Origin?.Code) == true &&
                        requirementsSlice.Destination?.Code.Is(bookingSlice?.Destination?.Code) == true);
                    var sliceRequirement = passengerRequirements?.SliceRequirements?.FirstOrDefault(requirement =>
                        requirement.SliceId == travelRequirementsSlice?.SliceId);

                    return (Slice)(x, cabinDetails, null, sliceRequirement);
                }),
                BirthDate = DateTime.TryParse(passenger.DateOfBirth, out var d) ? LocalDate.FromDateTime(d) : null,
                LeadPassenger = passenger.IsLeadPassenger
            };
        }

        public static explicit operator Passenger(Infrastructure.Models.HolidaysMS.TravellerParty party)
        {
            if (party == null) return null;

            return new()
            {
                FirstName = party?.FirstName,
                LastName = party?.LastName,
                Gender = party?.Gender,
                BirthDate = LocalDate.FromDateTime(party.DateOfBirth.Value),
                LeadPassenger = party.LeadTraveller
            };
        }
    }
}
