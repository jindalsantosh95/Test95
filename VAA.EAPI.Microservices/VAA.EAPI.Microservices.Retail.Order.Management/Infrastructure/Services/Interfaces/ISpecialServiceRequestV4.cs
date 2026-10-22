using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Proxy.SpecialServiceRequestV4Proxy;
namespace VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Services.Interfaces
{
    public interface ISpecialServiceRequestV4
    {
        AddResponseType AddSSRsRequest(AddRequestType addRequest);
    }
}
