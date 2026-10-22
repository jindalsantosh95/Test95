using CSharpFunctionalExtensions;
using NodaTime;
using NodaTime.Extensions;
using System.Globalization;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Helper;

namespace VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Domain
{
    public class Passenger
    {
        public string PassengerId { get; set; }
        public string? PassengerFirstNin { get; set; }
        public string? PassengerLastNin { get; set; }
        public string? GivenNames { get; set; }
        public string? Surname { get; set; }
        public string? AssociatedAdultFullName { get; set; }
        public string? AssociatedAdultPassengerId { get; set; }
        public bool? IsInfantPassenger { get; set; }
        public LocalDate DateOfBirth { get; set; }
        public string? Email { get; set; }
        public Phone? Phone { get; set; }
        public LoyaltyMembership? LoyaltyMembership { get; set; }
        public IEnumerable<Document> Documents { get; set; }
        public IEnumerable<Slice> Slices { get; set; }
        public CheckInRequirements? Requirements { get; set; }
        public IEnumerable<CheckInRequirements>? SliceRequirements { get; set; }
        public EmergencyContact.EmergencyContact? EmergencyContact { get; set; }
        public ContactTracing.ContactTracing? ContactTracing { get; set; }

        public static explicit operator Passenger((DeltaTrm.Passenger passenger, IEnumerable<DeltaTrm.Trip> trmTrips) source) => (Passenger)(null, source.passenger, source.trmTrips, null, null, null, null);


        public static explicit operator Passenger((DeltaSsci.Passenger1? ssciPassenger, DeltaTrm.Passenger? trmPassenger, IEnumerable<DeltaTrm.Trip> trmTrips, 
            string associatedPaxid, DeltaSsci.AddPassportResponse? ssciAddPassport, PassengerId? selectedPassengerId, bool? isInboundToUs) source)
        {
            var documents = new List<Document> { };

            var passport = source.trmPassenger?.AdvancePassengerInformation.First().TravelDocuments.FirstOrDefault(td => td.Type == DeltaTrm.IdentityDocumentType.PASSPORT);
            if (passport is not null)
            {
                documents.Add((Document)passport);
            }

            DateTime.TryParseExact(source.trmPassenger?.BirthDate, "yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateOfBirth);

            // We can check multiple TRM trips because they may relate to the same SSCI trip.
            var trmPassengerTripHealthRequirements = source.trmPassenger
                ?.AdvancePassengerInformation.SingleOrDefault()
                ?.PassengerTripHealthRequirements.Where(pthr => source.trmTrips.Any(t => t.Id == pthr.TripId));

            var trmContactInfo = source.trmPassenger
                ?.AdvancePassengerInformation?.SingleOrDefault()
                ?.ContactInformation?.SingleOrDefault();

            var trmPhone = trmContactInfo?.Phone?.FirstOrDefault();

            var trmEmergencyContact = source.trmPassenger?.AdvancePassengerInformation?.First()?.EmergencyContact?.First();
            var trmContactTracing = source.trmPassenger?.AdvancePassengerInformation?.First()?.ContactInformation?.First();

            IEnumerable<CheckInRequirements>? sliceRequirements = null;
            CheckInRequirements? requirements = null;
            
            if (source.ssciPassenger == null)
            {
                sliceRequirements = trmPassengerTripHealthRequirements?.Select(pthr => (CheckInRequirements)(new[] { pthr }, source.trmTrips, trmContactTracing, passport?.Nationality));
            }
            else
            {
                requirements = (CheckInRequirements)(source.ssciPassenger, trmPassengerTripHealthRequirements, source.trmTrips, trmContactTracing, source.ssciAddPassport, source.selectedPassengerId, source.isInboundToUs, passport?.Nationality);
            }
            

            return new Passenger()
            {
                PassengerId = source.ssciPassenger != null ? $"{source.ssciPassenger.FirstNIN}.{source.ssciPassenger.LastNIN}|{source.ssciPassenger.Id}|{source.ssciPassenger.CustomerId}|{source.trmPassenger?.PassengerResourceId}" : $"{source.trmPassenger?.NameNum}|{source.trmPassenger?.PassengerResourceId}",
                PassengerFirstNin = source.trmPassenger?.NameNum.Split('.').Skip(1).FirstOrDefault(),
                PassengerLastNin = source.trmPassenger?.NameNum.Split('.').FirstOrDefault(),
                GivenNames = source.trmPassenger?.GivenNames,
                Surname = source.trmPassenger?.Surname,
                AssociatedAdultFullName = source.ssciPassenger?.AssociatedAdultFullName,
                IsInfantPassenger = source.trmPassenger?.Infant ?? source.ssciPassenger?.InfantPassenger,
                AssociatedAdultPassengerId = source.associatedPaxid,
                DateOfBirth = dateOfBirth.ToLocalDateTime().Date,
                LoyaltyMembership = source.ssciPassenger?.Loyalty != null ? (LoyaltyMembership)source.ssciPassenger.Loyalty : null,
                Documents = documents,
                Email = trmContactInfo?.Email?.FirstOrDefault()?.Value,
                Phone = trmPhone != null ? (Phone)trmPhone : null,
                Requirements = requirements,
                SliceRequirements = sliceRequirements,
                EmergencyContact = trmEmergencyContact != null ? (EmergencyContact.EmergencyContact)trmEmergencyContact : null,
                ContactTracing = trmContactTracing != null ? (ContactTracing.ContactTracing)trmContactTracing : null
            };
        }
    }
}
