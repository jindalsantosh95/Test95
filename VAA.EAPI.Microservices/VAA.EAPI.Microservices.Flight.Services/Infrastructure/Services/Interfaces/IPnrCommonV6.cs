using VAA.EAPI.Microservices.Flight.Services.Infrastructure.Proxy.PNRCommon;

namespace VAA.EAPI.Microservices.Flight.Services.Infrastructure.Services.Interfaces
{
    public interface IPnrCommonV6
    {
        SetupContextResponseType Air4SetupContext(SetupContextRequestType setupContextRequest);
        TeardownContextResponseType Air4TeardownContext(TeardownContextRequestType teardownContextRequest);
    }
}
