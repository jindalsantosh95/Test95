namespace VAA.EAPI.Microservices.Retail.Passenger.Domain.Entities
{
    public class ManageMultipleTravelDocsAndSecureFlightRequestType
    {
        public RequestInfoType RequestInfo { get; set; }
        public SetupContextRequestType1 SetupContextRequest { get; set; }

        public PassengerNameItemType PassengerNameItem { get; set; }

        public DocumentDetailDataType[] DocumentDetailData { get; set; }

        public SecureFlightPassengerDataType SecureFlightPassengerData { get; set; }

        public GovtIssuedNumberType[] GovtIssuedNumber { get; set; }

        public PassengerContactDataType PassengerContactData { get; set; }

        public FlightInfoType[] FlightInfo { get; set; }

        public TeardownContextRequestType1 TeardownContextRequest { get; set; }

        public PnrRequestParamsType PnrRequestParams { get; set; }

        public StampingInfoType StampingInfo { get; set; }
    }
}
