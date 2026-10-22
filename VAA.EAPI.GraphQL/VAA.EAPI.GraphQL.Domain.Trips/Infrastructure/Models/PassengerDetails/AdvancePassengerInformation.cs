namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.PassengerDetails
{
    public class AdvancePassengerInformation
    {
        public IEnumerable<TrustedTravelerProgram> TrustedTravelerPrograms { get; set; }
        public ContactInformation PassengerContact { get; set; }
        public ContactInformation EmergencyContact { get; set; }
        public PostalAddress DestinationAddress { get; set; }
        public IEnumerable<AdvancePassengerInformationRequirement> AdvancePassengerInformationRequirements { get; set; }
    }
}