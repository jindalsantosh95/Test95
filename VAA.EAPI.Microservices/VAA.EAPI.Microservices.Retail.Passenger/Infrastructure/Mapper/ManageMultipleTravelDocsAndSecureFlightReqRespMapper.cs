using VAA.EAPI.Microservices.Retail.Passenger.Infrastructure.Proxy;

namespace VAA.EAPI.Microservices.Retail.Passenger.Infrastructure.Mapper
{
    public class ManageMultipleTravelDocsAndSecureFlightReqRespMapper
    {
        public static ManageMultipleTravelDocsAndSecureFlightRequestType ManageMultipleTravelDocsAndSecureFlightReq(Domain.Entities.ManageMultipleTravelDocsAndSecureFlightRequestType manageMultipleTravelDocsAndSecureFlightRequestType)
        {
            var mapper = PassengerMapperConfiguration.MapperConfig();
            return new ManageMultipleTravelDocsAndSecureFlightRequestType()
            {
                RequestInfo = mapper.Map<RequestInfoType>(manageMultipleTravelDocsAndSecureFlightRequestType.RequestInfo),
                StampingInfo = mapper.Map<StampingInfoType>(manageMultipleTravelDocsAndSecureFlightRequestType.StampingInfo),
                DocumentDetailData = mapper.Map<DocumentDetailDataType[]>(manageMultipleTravelDocsAndSecureFlightRequestType.DocumentDetailData),
                FlightInfo = mapper.Map<FlightInfoType[]>(manageMultipleTravelDocsAndSecureFlightRequestType.FlightInfo),
                GovtIssuedNumber = mapper.Map<GovtIssuedNumberType[]>(manageMultipleTravelDocsAndSecureFlightRequestType.GovtIssuedNumber),
                PassengerContactData = mapper.Map<PassengerContactDataType>(manageMultipleTravelDocsAndSecureFlightRequestType.PassengerContactData),
                PassengerNameItem = mapper.Map<PassengerNameItemType>(manageMultipleTravelDocsAndSecureFlightRequestType.PassengerNameItem),
                PnrRequestParams = mapper.Map<PnrRequestParamsType>(manageMultipleTravelDocsAndSecureFlightRequestType.PnrRequestParams),
                SecureFlightPassengerData = mapper.Map<SecureFlightPassengerDataType>(manageMultipleTravelDocsAndSecureFlightRequestType.SecureFlightPassengerData),
                SetupContextRequest = mapper.Map<SetupContextRequestType1>(manageMultipleTravelDocsAndSecureFlightRequestType.SetupContextRequest),
                TeardownContextRequest = mapper.Map<TeardownContextRequestType1>(manageMultipleTravelDocsAndSecureFlightRequestType.TeardownContextRequest),
            };
        }

        public static Domain.Entities.ManageMultipleTravelDocsAndSecureFlightResponseType ManageMultipleTravelDocsAndSecureFlightRes(ManageMultipleTravelDocsAndSecureFlightResponse addResponseType)
        {
            var mapper = PassengerMapperConfiguration.MapperConfig();
            return new Domain.Entities.ManageMultipleTravelDocsAndSecureFlightResponseType()
            {
                AdvisoryMessage = mapper.Map<Domain.Entities.AdvisoryMessageType[]>(addResponseType.ManageMultipleTravelDocsAndSecureFlightResponse1.AdvisoryMessage),
                DocumentDetailData = mapper.Map<Domain.Entities.DocumentDetailDataType[]>(addResponseType.ManageMultipleTravelDocsAndSecureFlightResponse1.DocumentDetailData),
                GovtIssuedNumber = mapper.Map<Domain.Entities.GovtIssuedNumberType[]>(addResponseType.ManageMultipleTravelDocsAndSecureFlightResponse1.GovtIssuedNumber),
                PassengerAddressInfo = mapper.Map<Domain.Entities.PassengerAddressInfoType>(addResponseType.ManageMultipleTravelDocsAndSecureFlightResponse1.PassengerAddressInfo),
                PassengerNameItem = mapper.Map<Domain.Entities.PassengerNameItemType>(addResponseType.ManageMultipleTravelDocsAndSecureFlightResponse1.PassengerNameItem),
                ResponseInfo = mapper.Map<Domain.Entities.ResponseInfoType>(addResponseType.ManageMultipleTravelDocsAndSecureFlightResponse1.ResponseInfo),
                SecureFlightPassengerData = mapper.Map<Domain.Entities.SecureFlightPassengerDataType>(addResponseType.ManageMultipleTravelDocsAndSecureFlightResponse1.SecureFlightPassengerData),
                TeardownContextResponse = mapper.Map<Domain.Entities.TeardownContextResponseType1>(addResponseType.ManageMultipleTravelDocsAndSecureFlightResponse1.TeardownContextResponse),
            };
        }
    }
}
