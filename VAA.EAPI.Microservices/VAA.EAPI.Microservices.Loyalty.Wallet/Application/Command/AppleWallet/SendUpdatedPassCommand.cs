using MediatR;
using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.Microservices.Loyalty.Wallet.Application.Command.AppleWallet
{
    public class SendUpdatedPassCommand : IRequest<Response<byte[]>>
    {
        public string FcNumber { get; set; }
    }
}
