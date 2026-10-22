using MediatR;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.SabreCommandLLS;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Repositories
{
    public class SabreCommandLLSCommand : IRequest<SabreCommandLLSRS>
    {
        public SabreCommandLLSRQRequest1 SabreCommandLLSRequest { get; set; }
    }
}
