using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Wallet.Domain;

namespace VAA.EAPI.Microservices.Loyalty.Wallet.Application.Query
{
    public class GetPassQuery : IRequest<Response<AssociatedPasses>>
    {
        public string DeviceId { get; set; }
        public string PassTypeId { get; set; }
    }
}
