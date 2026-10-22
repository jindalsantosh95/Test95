using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.RetroCredit;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.Command.RetroCredit
{
    public class RetroCreditCommand : IRequest<Response<RetroCreditResponse>>
    {
        public RetroCreditRequest RetroCreditRequest { get; set; }
    }
}
