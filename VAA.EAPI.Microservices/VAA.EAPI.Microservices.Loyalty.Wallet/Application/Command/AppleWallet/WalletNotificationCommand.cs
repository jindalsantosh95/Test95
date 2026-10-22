using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Events;

namespace VAA.EAPI.Microservices.Loyalty.Wallet.Application.Command.AppleWallet
{
    public class WalletNotificationCommand : CloudEvent, IRequest<Result<Unit>>
    {
    }
}
