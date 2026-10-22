using MediatR;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.CreatePNR;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Repositories
{
    public class CreatePNRCommand : IRequest<CreatePassengerNameRecordRS>
    {
        public CreatePassengerNameRecordRQRequest CreatePNRRequest { get; set; }
    }
}
