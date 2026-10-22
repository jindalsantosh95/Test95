using VAA.EAPI.Microservices.Retail.Order.Infrastructure.Proxy.SpecialServiceRequestV4Proxy;

namespace VAA.EAPI.Microservices.Retail.Order.Infrastructure.Services.Interfaces
{
    public interface ISpecialServiceRequestV4
    {
        AddResponseType AddSSRsRequest(AddRequestType addRequest);
    }
}
