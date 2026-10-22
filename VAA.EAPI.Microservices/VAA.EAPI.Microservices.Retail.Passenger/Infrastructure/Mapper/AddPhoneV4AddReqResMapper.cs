using VAA.EAPI.Microservices.Retail.Passenger.Infrastructure.Proxy;

namespace VAA.EAPI.Microservices.Retail.Passenger.Infrastructure.Mapper
{
    public class AddPhoneV4AddReqResMapper
    {
        public static AddPhoneRequestType AddPhoneV4AddReq(Domain.Entities.AddPhoneRequestType addPhoneRequestType)
        {
            var mapper = PassengerMapperConfiguration.MapperConfig();
            return new AddPhoneRequestType()
            {
                RequestInfo = mapper.Map<RequestInfoType>(addPhoneRequestType.RequestInfo),
                PhoneInfo = mapper.Map<PhoneInfoType[]>(addPhoneRequestType.PhoneInfo),
                StampingInfo = mapper.Map<StampingInfoType>(addPhoneRequestType.StampingInfo),
                PnrRequestParams = mapper.Map<PnrRequestParamsType>(addPhoneRequestType.PnrRequestParams),
                RequestOption = mapper.Map<RequestOptionType>(addPhoneRequestType.RequestOption)
            };
        }
        public static Domain.Entities.AddPhoneResponseType AddPhoneV4AddRes(AddPhoneResponseType addPhoneResponseType)
        {
            var mapper = PassengerMapperConfiguration.MapperConfig();
            return new Domain.Entities.AddPhoneResponseType()
            {
                Error = mapper.Map<Domain.Entities.ErrorType>(addPhoneResponseType.Error),
                Pnr = mapper.Map<Domain.Entities.PnrType>(addPhoneResponseType.Pnr),
                AddPhoneErrorList = mapper.Map<Domain.Entities.AddPhoneErrorListType[]>(addPhoneResponseType.AddPhoneErrorList),
                ResponseInfo = mapper.Map<Domain.Entities.ResponseInfoType>(addPhoneResponseType.ResponseInfo),

            };
        }
    }
}
