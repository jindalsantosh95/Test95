using VAA.EAPI.Microservices.Retail.Passenger.Infrastructure.Proxy;

namespace VAA.EAPI.Microservices.Retail.Passenger.Infrastructure.Mapper
{
    public class PassengerV4AddReqResMapper
    {
        public static AddRequestType PassengerV4AddReq(Domain.Entities.AddRequestType addRequestType)
        {
            var mapper = PassengerMapperConfiguration.MapperConfig();
            return new AddRequestType()
            {
                RequestInfo = mapper.Map<RequestInfoType>(addRequestType.RequestInfo),
                StampingInfo = mapper.Map<StampingInfoType>(addRequestType.StampingInfo),
                PassengerInfo = mapper.Map<PassengerInfoType[]>(addRequestType.PassengerInfo),
            };
        }

        public static Domain.Entities.AddResponseType PassengerV4AddRes(AddResponse addResponseType)
        {
            var mapper = PassengerMapperConfiguration.MapperConfig();
            return new Domain.Entities.AddResponseType()
            {
                Error = mapper.Map<Domain.Entities.ErrorType>(addResponseType.AddResponse1.Error),
                Pnr = mapper.Map<Domain.Entities.PnrType>(addResponseType.AddResponse1.Pnr),
                AddErrorList = mapper.Map<Domain.Entities.AddErrorListType[]>(addResponseType.AddResponse1.AddErrorList),
                ResponseInfo = mapper.Map<Domain.Entities.ResponseInfoType>(addResponseType.AddResponse1.ResponseInfo),
            };
        }

        //public static  PassengerProxy.AddRequestType AssignChannelInformation(PassengerProxy.AddRequestType addRequestType, string applicationId, string applicationName)
        //{

        //    addRequestType.RequestInfo.ApplicationId =  applicationId;
        //    addRequestType.RequestInfo.AppChannelName = applicationName;
        //    return addRequestType;
        //}
    }
}
