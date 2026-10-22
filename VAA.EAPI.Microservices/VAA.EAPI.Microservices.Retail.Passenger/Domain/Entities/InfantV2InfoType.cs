namespace VAA.EAPI.Microservices.Retail.Passenger.Domain.Entities
{
    public class InfantV2InfoType
    {
        public InfantProcessRequestOptionsType InfantProcessRequestOptions { get; set; }
        public InfantPassengerInfoType[] InfantPassengerInfo { get; set; }
        public SeatEligiblePassengerInfoType[] SeatEligiblePassengerInfo { get; set; }
        public GovtIssuedNumberType1[] GovtIssuedNumber { get; set; }
        public SecureFlightPassengerDataType1 SecureFlightPassengerData { get; set; }
        public InfantNameManagementType InfantNameManagement { get; set; }
        public InfantSSRFlightDataType[] InfantSSRFlightData { get; set; }
    }
}
