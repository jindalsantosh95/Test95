using MediatR;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.GetElectronicDocument;
using VAA.EAPI.Infrastructure.NDC.Sabre.Services.Interfaces;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Repositories
{
    public class GetElectronicDocumentQueryHandler(IGetElectronicDocument getElectronicDocument) : IRequestHandler<GetElectronicDocumentQuery, GetElectronicDocumentRS>
    {
        private IGetElectronicDocument _getElectronicDocument = getElectronicDocument;

        public async Task<GetElectronicDocumentRS> Handle(GetElectronicDocumentQuery request, CancellationToken cancellationToken)
        {
            return await _getElectronicDocument.GetElectronicDocumentRQAsync(request.GetElectronicDocumentRQRequest);
        }
    }
}
