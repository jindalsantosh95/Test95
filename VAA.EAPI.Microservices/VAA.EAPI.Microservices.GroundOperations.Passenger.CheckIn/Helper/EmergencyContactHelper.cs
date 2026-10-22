using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Domain.EmergencyContact;
using CheckInPassenger = VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Domain.Passenger;

namespace VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Helper
{
    public static class EmergencyContactHelper
    {
        public static bool ShouldApplyEmergencyContactToAllPassengers(IEnumerable<CheckInPassenger> passengers)
        {
            var pax = passengers?.ToList() ?? [];

            if (pax.Count <= 1)
                return false;

            if (pax.All(passenger => IsEmergencyContactBlank(passenger?.EmergencyContact)))
                return true;

            var passengersWithEmergencyContact = pax.Where(passenger =>
                passenger?.IsInfantPassenger != true &&
                !IsEmergencyContactBlank(passenger?.EmergencyContact)).ToList();

            if (passengersWithEmergencyContact.Count < 2)
                return true;

            return CompareEmergencyContactDetails(passengersWithEmergencyContact);
        }

        private static bool IsEmergencyContactBlank(EmergencyContact? emergencyContact) =>
            emergencyContact is null ||
            (emergencyContact.FirstName.IsNullOrWhiteSpace() &&
             emergencyContact.LastName.IsNullOrWhiteSpace() &&
             emergencyContact.PhoneCountryCode.IsNullOrWhiteSpace() &&
             emergencyContact.PhoneNumber.IsNullOrWhiteSpace());

        private static bool CompareEmergencyContactDetails(List<CheckInPassenger> passengersWithEmergencyContact)
        {
            var firstEmergencyContact = passengersWithEmergencyContact.First().EmergencyContact;

            if (firstEmergencyContact is null)
                return false;

            static string Normalize(string? value) => value?.Trim() ?? string.Empty;

            var firstFirstName = Normalize(firstEmergencyContact.FirstName);
            var firstLastName = Normalize(firstEmergencyContact.LastName);
            var firstPhoneCountryCode = Normalize(firstEmergencyContact.PhoneCountryCode);
            var firstPhoneNumber = Normalize(firstEmergencyContact.PhoneNumber);
            var firstPassengerRefused = firstEmergencyContact.PassengerRefused;

            return passengersWithEmergencyContact.All(passenger =>
            {
                var emergencyContact = passenger.EmergencyContact;

                if (emergencyContact is null)
                    return false;

                return Normalize(emergencyContact.FirstName).Is(firstFirstName)
                    && Normalize(emergencyContact.LastName).Is(firstLastName)
                    && Normalize(emergencyContact.PhoneCountryCode).Is(firstPhoneCountryCode)
                    && Normalize(emergencyContact.PhoneNumber).Is(firstPhoneNumber)
                    && emergencyContact.PassengerRefused == firstPassengerRefused;
            });
        }
    }
}
