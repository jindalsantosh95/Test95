
namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaPassengerInfo
{
    public class AdvancePassengerInformation
    {
        public IEnumerable<TrustedTravelerProgram> TrustedTravelerPrograms { get; set; }
        public ContactInformation PassengerContact { get; set; }
        public ContactInformation EmergencyContact { get; set; }
        public PostalAddress DestinationAddress { get; set; }
        public IEnumerable<AdvancePassengerInformationRequirement> AdvancePassengerInformationRequirements { get; set; }

        public static explicit operator AdvancePassengerInformation(Domain.PassengerDetails.AdvancePassengerInformation source)
        {
            if (source is null) return null;

            return new AdvancePassengerInformation()
            {
                TrustedTravelerPrograms = source.TrustedTravelerPrograms?.Select(x => (TrustedTravelerProgram)x),
                PassengerContact = (ContactInformation)source.PassengerContact,
                EmergencyContact = (ContactInformation)source.EmergencyContact,
                DestinationAddress = (PostalAddress)source.DestinationAddress,
                AdvancePassengerInformationRequirements = source.AdvancePassengerInformationRequirements?.Select(x => (AdvancePassengerInformationRequirement)x)
            };
        }
    }
}