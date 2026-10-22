using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.PSS.Proxy.HostContext;

namespace VAA.EAPI.Infrastructure.PSS.Services.Interfaces
{
    public interface IAir4HostContextV2
    {
        Task<CreateAndSwingResponse> CreateAndSwing(CreateAndSwingRequestType createAndSwingRequest);
        Task<CreateResponse> Create(CreateRequestType createRequestType);
        Task<SwingCityDutyCodeResponse> Swing(SwingCityDutyCodeRequestType swingCityDutyCodeRequestType);

    }
}
