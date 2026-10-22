using MediatR;
using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.Microservices.Loyalty.Wallet.Application.Command.AppleWallet
{
    public class WalletEventNotificationCommand : IRequest<Response<string>>
    {
    }
}
