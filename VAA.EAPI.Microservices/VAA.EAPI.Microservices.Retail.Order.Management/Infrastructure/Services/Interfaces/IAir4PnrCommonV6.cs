using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Proxy.PNRCommonV6Proxy;
namespace VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Services.Interfaces
{
    public interface IAir4PnrCommonV6
    {
        SetupContextResponseType Air4SetupContext(SetupContextRequestType setupContextRequest);
        TeardownContextResponseType Air4TearDownContext(TeardownContextRequestType teardownContextRequest);
    }
}
