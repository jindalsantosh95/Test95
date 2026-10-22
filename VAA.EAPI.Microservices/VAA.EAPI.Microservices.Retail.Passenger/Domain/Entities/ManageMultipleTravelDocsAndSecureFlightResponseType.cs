namespace VAA.EAPI.Microservices.Retail.Passenger.Domain.Entities
{
    public class ManageMultipleTravelDocsAndSecureFlightResponseType
    {
        public TeardownContextResponseType1 TeardownContextResponse { get; set; }
        public ResponseInfoType ResponseInfo { get; set; }
        public PassengerNameItemType PassengerNameItem { get; set; }
        public DocumentDetailDataType[] DocumentDetailData { get; set; }
        public SecureFlightPassengerDataType SecureFlightPassengerData { get; set; }
        public GovtIssuedNumberType[] GovtIssuedNumber { get; set; }
        public PassengerAddressInfoType PassengerAddressInfo { get; set; }
        public AdvisoryMessageType[] AdvisoryMessage { get; set; }
    }
}
