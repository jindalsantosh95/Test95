using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.SabreCommandLLS;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Services.Interfaces
{
    public interface ISabreCommandLLS
    {
        Task<SabreCommandLLSRS> SabreCommandLLSAsync(SabreCommandLLSRQRequest1 request);
    }
}
