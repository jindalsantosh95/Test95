using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.OTACancel;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Services.Interfaces
{
    public interface IOTACancelLLS
    {
        Task<OTA_CancelRQResponse> OTACancelLLSAsync(OTA_CancelRQRequest request);
    }
}
