namespace VAA.EAPI.Microservices.Retail.Passenger.Domain.Entities
{
    public class InfantV2ResponseInfoType
    {
        public DebugInfoType DebugInfo { get; set; }
        public ResponseInfoType ResponseInfo { get; set; }
        public SeatEligiblePassengerInfoType[] SeatEligiblePassengerInfo { get; set; }
        public InfantPassengerInfoType[] InfantPassengerInfo { get; set; }
        public GovtIssuedNumberType1[] GovtIssuedNumber { get; set; }
        public SecureFlightPassengerDataType1 SecureFlightPassengerData { get; set; }
        public InfantNameManagementType InfantNameManagement { get; set; }
        public InfantSSRFlightDataType[] InfantSSRFlightData { get; set; }
        public AdvisoryMessageType[] AdvisoryMessage { get; set; }
        public ErrorType[] Error { get; set; }
        public bool ManageInfantSuccess { get; set; }
        public bool ManageInfantSuccessSpecified { get; set; }
    }
}
