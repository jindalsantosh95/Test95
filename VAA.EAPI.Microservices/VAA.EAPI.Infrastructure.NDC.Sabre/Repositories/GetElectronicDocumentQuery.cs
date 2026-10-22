using MediatR;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.GetElectronicDocument;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Repositories
{
    public class GetElectronicDocumentQuery : IRequest<GetElectronicDocumentRS>
    {
        public GetElectronicDocumentRQRequest  GetElectronicDocumentRQRequest {  get; set; }
    }
}
