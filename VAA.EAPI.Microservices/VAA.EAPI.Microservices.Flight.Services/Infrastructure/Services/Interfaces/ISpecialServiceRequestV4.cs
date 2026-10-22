using VAA.EAPI.Microservices.Flight.Services.Infrastructure.Proxy.SpecialService;

namespace VAA.EAPI.Microservices.Flight.Services.Infrastructure.Services.Interfaces
{
    public interface ISpecialServiceRequestV4
    {
        AddResponseType AddSSRsRequest(AddRequestType addRequest);
        DeleteResponseType DeleteSSRsRequest(DeleteRequestType deleteRequest);
    }
}
