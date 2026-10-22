
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.ManualPriceQuoteAddInfoLLS;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Services.Interfaces
{
    public interface IManualPriceQuoteAddInfo 
    {       
        Task<ManualPriceQuoteAddInfoRS> ManualPriceQuoteAddInfoRQAsync(ManualPriceQuoteAddInfoRQRequest manualPriceQuoteAddInfoRQRequest);
    }
}
