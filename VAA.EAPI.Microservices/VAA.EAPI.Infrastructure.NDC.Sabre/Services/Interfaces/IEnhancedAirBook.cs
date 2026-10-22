using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.EnhancedAirBook;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Services.Interfaces
{
    public interface IEnhancedAirBook
    {
        Task<EnhancedAirBookRQResponse> EnhancedAirBookAsync(EnhancedAirBookRQRequest request);
    }
}
