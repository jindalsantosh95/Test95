using MediatR;
using VAA.EAPI.Domain.Entities.Common;

namespace VAA.Facade.WalletFacade.Application.Command
{
    public class SendUpdatedPassCommand : IRequest<byte[]>
    {
        public string FcNumber { get; set; }
    }
}
