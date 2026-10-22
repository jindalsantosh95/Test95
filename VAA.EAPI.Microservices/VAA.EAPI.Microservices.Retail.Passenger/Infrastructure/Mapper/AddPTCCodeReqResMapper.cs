
using VAA.EAPI.Microservices.Retail.Passenger.Infrastructure.Proxy;

namespace VAA.EAPI.Microservices.Retail.Passenger.Infrastructure.Mapper
{
    public class AddPTCCodeReqResMapper
    {
        public static AddPTCCodesRequestType AddPTCCodesRequestTypeMapper(Domain.Entities.AddPTCCodesRequestType addPTCCodesRequestType)
        {
            var mapper = PassengerMapperConfiguration.MapperConfig();
            return new AddPTCCodesRequestType()
            {
                RequestInfo = mapper.Map<RequestInfoType>(addPTCCodesRequestType.RequestInfo),
                PnrRequestParams = mapper.Map<PnrRequestParamsType>(addPTCCodesRequestType.PnrRequestParams),
                PTCCode = mapper.Map<PTCCodeType[]>(addPTCCodesRequestType.PTCCode),
                StampingInfo = mapper.Map<StampingInfoType>(addPTCCodesRequestType.StampingInfo),
                RequestOption = mapper.Map<RequestOptionType>(addPTCCodesRequestType.RequestOption)
            };
        }
        public static Domain.Entities.AddPTCCodesResponseType AddPTCCodesResponseTypeMapper(AddPTCCodesResponse addPTCCodeResponse)
        {
            var mapper = PassengerMapperConfiguration.MapperConfig();
            return new Domain.Entities.AddPTCCodesResponseType()
            {
                Pnr = mapper.Map<Domain.Entities.PnrType>(addPTCCodeResponse.AddPTCCodesResponse1.Pnr),
                ResponseInfo = mapper.Map<Domain.Entities.ResponseInfoType>(addPTCCodeResponse.AddPTCCodesResponse1.ResponseInfo)
            };
        }
    }
}
