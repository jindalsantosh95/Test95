using System.ComponentModel.DataAnnotations;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.DeltaTrm;
using PassengerIdHelper = VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Helper.PassengerId;

namespace VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Domain.TravelRequirements
{
    public class SaveContactTracingRequest
    {
        [Required]
        public string RequirementsId { get; set; }

        [Required]
        public string SliceId { get; set; }

        [Required]
        public IEnumerable<ContactTracing.ContactTracing> ContactTracing { get; set; }

        public static explicit operator RequirementFormRequest(SaveContactTracingRequest source)
        {
            var contactTracingList = source.ContactTracing?.ToList();
            Validate(source, contactTracingList);

            var requests = new List<RequirementFormRequest>();
            foreach (var contactTracing in contactTracingList)
            {
                var passengerId = PassengerIdHelper.FromTravelRequirements(contactTracing.PassengerId);

                requests.Add(new RequirementFormRequest
                {
                    PassengerResourceId = passengerId.TrmPassengerResourceId,
                    TripId = source.SliceId,
                    Using = RequirementType.CONTACT_TRACING,
                    Consent = true,
                    ContactInformation = new List<ContactInformation> { (ContactInformation)contactTracing }
                });
            }

            return new RequirementFormRequest
            {
                Requests = requests
            };
        }

        private static void Validate(SaveContactTracingRequest source, IReadOnlyCollection<ContactTracing.ContactTracing> contactTracingList)
        {
            if (string.IsNullOrWhiteSpace(source.RequirementsId))
            {
                throw new ValidationException("RequirementsId is required.");
            }

            if (string.IsNullOrWhiteSpace(source.SliceId))
            {
                throw new ValidationException("SliceId is required.");
            }

            if (contactTracingList?.Any() != true)
            {
                throw new ValidationException("ContactTracing is required.");
            }

            foreach (var contactTracing in contactTracingList)
            {
                ValidateContactTracing(contactTracing);
            }
        }

        private static void ValidateContactTracing(ContactTracing.ContactTracing contactTracing)
        {
            if (contactTracing is null)
            {
                throw new ValidationException("ContactTracing cannot contain null entries.");
            }

            if (string.IsNullOrWhiteSpace(contactTracing.PassengerId))
            {
                throw new ValidationException("ContactTracing.PassengerId is required.");
            }

            if (contactTracing.InTransit != true)
            {
                if (string.IsNullOrWhiteSpace(contactTracing.StreetAddress))
                {
                    throw new ValidationException("ContactTracing.StreetAddress is required.");
                }

                if (string.IsNullOrWhiteSpace(contactTracing.City))
                {
                    throw new ValidationException("ContactTracing.City is required.");
                }

                if (string.IsNullOrWhiteSpace(contactTracing.State))
                {
                    throw new ValidationException("ContactTracing.State is required.");
                }

                if (string.IsNullOrWhiteSpace(contactTracing.Zip))
                {
                    throw new ValidationException("ContactTracing.Zip is required.");
                }
            }

            if (string.IsNullOrWhiteSpace(contactTracing.PrimaryPhoneNumber))
            {
                throw new ValidationException("ContactTracing.PrimaryPhoneNumber is required.");
            }

            if (string.IsNullOrWhiteSpace(contactTracing.PrimaryPhoneNumberCountryCode))
            {
                throw new ValidationException("ContactTracing.PrimaryPhoneNumberCountryCode is required.");
            }

        }
    }
}
