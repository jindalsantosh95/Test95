namespace VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Domain.EmergencyContact
{
    public class EmergencyContact
    {
        public string? PassengerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [System.ComponentModel.DataAnnotations.RegularExpression(@"^[A-Z]{2}$", ErrorMessage = "PhoneCountryCode must be an ISO-2 country code.")]
        public string PhoneCountryCode { get; set; }
        public string PhoneNumber { get; set; }
        public bool PassengerRefused { get; set; }

        public static explicit operator EmergencyContact(DeltaTrm.PhoneInfo phoneInfo)
        {
            return new EmergencyContact
            {
                FirstName = phoneInfo.FirstName,
                LastName = phoneInfo.LastName,
                PhoneCountryCode = phoneInfo.CountryCode,
                PhoneNumber = phoneInfo.PhoneNumber,
                PassengerRefused = phoneInfo.Declined
            };
        }
        public static explicit operator DeltaTrm.PhoneInfo(EmergencyContact request)
        {
            return new DeltaTrm.PhoneInfo
            {
                Declined = request.PassengerRefused,
                Type = DeltaTrm.PhoneInfoType.MOBILE,
                FirstName = request?.FirstName,
                LastName = request?.LastName,
                CountryCode = request?.PhoneCountryCode,
                PhoneNumber = request?.PhoneNumber
            };
        }
        public static bool operator ==(EmergencyContact a, EmergencyContact b)
        {
            return a.FirstName == b.FirstName &&
                   a.LastName == b.LastName &&
                   a.PhoneCountryCode == b.PhoneCountryCode &&
                   a.PhoneNumber == b.PhoneNumber &&
                   a.PassengerRefused == b.PassengerRefused;
        }

        public static bool operator !=(EmergencyContact a, EmergencyContact b)
        {
            return (a == b) == false;
        }

        public static bool Equals(EmergencyContact a, EmergencyContact b)
        {
            return a == b;
        }
    }
}
