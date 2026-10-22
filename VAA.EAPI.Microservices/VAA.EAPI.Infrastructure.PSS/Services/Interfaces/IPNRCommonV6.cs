using VAA.EAPI.Infrastructure.PSS.Proxy.PNRCommonV6Proxy;

namespace VAA.EAPI.Infrastructure.PSS.Services.Interfaces
{
    public interface IPNRCommonV6
    {
        RetrieveResponseType Air4Retrieve(RetrieveRequestType retrieveRequest);

        SetupContextResponseType Air4SetupContext(SetupContextRequestType setupContextRequest);
        TeardownContextResponseType Air4TeardownContext(TeardownContextRequestType teardownContextRequest);
    }
}
