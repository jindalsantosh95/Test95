using System.ComponentModel.DataAnnotations;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.DeltaTrm;
using PassengerIdHelper = VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Helper.PassengerId;

namespace VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Domain.TravelRequirements
{
    public class SaveEmergencyContactRequest
    {
        private const string Iso2CountryCodePattern = "^[A-Z]{2}$";

        [Required]
        public string RequirementsId { get; set; }

        [Required]
        public IEnumerable<EmergencyContact.EmergencyContact> EmergencyContacts { get; set; }

        public static explicit operator RequirementFormRequest(SaveEmergencyContactRequest source)
        {
            if (source.EmergencyContacts?.Any() != true)
            {
                throw new ValidationException("EmergencyContacts is required.");
            }

            var requests = new List<RequirementFormRequest>();
            foreach (var contact in source.EmergencyContacts)
            {
                ValidateEmergencyContact(contact);

                requests.Add(new RequirementFormRequest
                {
                    PassengerResourceId = PassengerIdHelper.FromTravelRequirements(contact.PassengerId).TrmPassengerResourceId,
                    Using = RequirementType.EMERGENCY_CONTACT,
                    EmergencyContact = [(PhoneInfo)contact]
                });
            }

            return new RequirementFormRequest
            {
                Requests = requests
            };
        }

        private static void ValidateEmergencyContact(EmergencyContact.EmergencyContact contact)
        {
            if (contact is null)
            {
                throw new ValidationException("EmergencyContacts cannot contain null entries.");
            }

            if (string.IsNullOrWhiteSpace(contact.PassengerId))
            {
                throw new ValidationException("EmergencyContacts.PassengerId is required.");
            }

            if (contact.PassengerRefused)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(contact.FirstName))
            {
                throw new ValidationException("EmergencyContacts.FirstName is required.");
            }

            if (string.IsNullOrWhiteSpace(contact.LastName))
            {
                throw new ValidationException("EmergencyContacts.LastName is required.");
            }

            if (string.IsNullOrWhiteSpace(contact.PhoneCountryCode))
            {
                throw new ValidationException("EmergencyContacts.PhoneCountryCode is required.");
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(contact.PhoneCountryCode, Iso2CountryCodePattern))
            {
                throw new ValidationException("EmergencyContacts.PhoneCountryCode must be an ISO-2 country code.");
            }

            if (string.IsNullOrWhiteSpace(contact.PhoneNumber))
            {
                throw new ValidationException("EmergencyContacts.PhoneNumber is required.");
            }
        }
    }
}
