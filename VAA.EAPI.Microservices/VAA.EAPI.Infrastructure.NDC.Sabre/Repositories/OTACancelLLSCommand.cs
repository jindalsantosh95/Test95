using MediatR;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.OTACancel;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Repositories
{
    public class OTACancelLLSCommand : IRequest<OTA_CancelRQResponse>
    {
        public OTA_CancelRQRequest OtaCancelRequest { get; set; }

    }
}
