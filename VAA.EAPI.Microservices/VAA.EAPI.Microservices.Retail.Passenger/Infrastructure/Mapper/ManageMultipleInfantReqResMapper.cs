using VAA.EAPI.Microservices.Retail.Passenger.Infrastructure.Proxy;

namespace VAA.EAPI.Microservices.Retail.Passenger.Infrastructure.Mapper
{
    public class ManageMultipleInfantReqResMapper
    {
        public static ManageMultipleInfantV2RequestType ManageMultipleInfantRequestTypeMapper(Domain.Entities.ManageMultipleInfantV2RequestType manageMultipleInfantRequestType)
        {
            var mapper = PassengerMapperConfiguration.MapperConfig();
            return new ManageMultipleInfantV2RequestType()
            {
                RequestInfo = mapper.Map<RequestInfoType>(manageMultipleInfantRequestType.RequestInfo),
                InfantV2Info = mapper.Map<InfantV2InfoType[]>(manageMultipleInfantRequestType.InfantV2Info),
            };
        }
        public static Domain.Entities.ManageMultipleInfantV2Response ManageMultipleInfantResponseTypeMapper(ManageMultipleInfantV2Response manageMultipleInfantResponseType)
        {
            var mapper = PassengerMapperConfiguration.MapperConfig();
            return new Domain.Entities.ManageMultipleInfantV2Response()
            {
                ManageMultipleInfantV2Response1 = mapper.Map<Domain.Entities.InfantV2ResponseInfoType[]>(manageMultipleInfantResponseType.ManageMultipleInfantV2Response1)
            };
        }
    }
}
