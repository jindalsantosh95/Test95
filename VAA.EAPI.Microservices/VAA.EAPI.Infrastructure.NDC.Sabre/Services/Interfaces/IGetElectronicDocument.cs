using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.GetElectronicDocument;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Services.Interfaces
{
    public interface IGetElectronicDocument 
    {
        Task<GetElectronicDocumentRS> GetElectronicDocumentRQAsync(GetElectronicDocumentRQRequest request);
    }
}
