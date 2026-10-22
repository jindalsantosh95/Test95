using MediatR;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.DesignatePrinter;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Repositories
{
    public class DesignatePrinterCommand : IRequest<DesignatePrinterRQResponse>
    {
        public DesignatePrinterRQRequest DesignatePrinterRequest { get; set; }
    }
}
