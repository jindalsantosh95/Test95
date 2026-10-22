using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.Facade.WalletFacade.Domain;

namespace VAA.Facade.WalletFacade.Application.Command
{
    public class GetPassCommand : IRequest<Response<AssociatedPasses>>
    {
        public string DeviceId { get; set; }
        public string PassTypeId { get; set; }        
    }
}
