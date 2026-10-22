using MediatR;
using VAA.Facade.PDMFacade.Domain;
using VAA.EAPI.Domain.Entities.Common;

namespace VAA.Facade.PDMFacade.Application.Command
{
    public class RetroCreditCommand : IRequest<Response<RetroCreditResponse>>
    {
        public RetroCreditRequest RetroCreditRequest { get; set; }

    }
}
