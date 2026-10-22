using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.DesignatePrinter;
using VAA.EAPI.Infrastructure.NDC.Sabre.Services.Interfaces;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Repositories
{
    public class DesignatePrinterCommandHandler(IDesignatePrinter designatePrinter) : IRequestHandler<DesignatePrinterCommand, DesignatePrinterRQResponse>
    {
        private readonly IDesignatePrinter _designatePrinter = designatePrinter ?? throw new ArgumentNullException(nameof(designatePrinter));

        public async Task<DesignatePrinterRQResponse> Handle(DesignatePrinterCommand request, CancellationToken cancellationToken)
        {
            return await _designatePrinter.DesignatePrinterAsync(request.DesignatePrinterRequest);
        }
    }
}
