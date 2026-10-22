using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.DesignatePrinter;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Services.Interfaces
{
    public interface IDesignatePrinter
    {
        Task<DesignatePrinterRQResponse> DesignatePrinterAsync(DesignatePrinterRQRequest designatePrinterRQ);
    }
}
