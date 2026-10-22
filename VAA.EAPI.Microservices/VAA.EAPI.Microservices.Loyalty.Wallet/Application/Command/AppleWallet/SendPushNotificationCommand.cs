using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Events;

namespace VAA.EAPI.Microservices.Loyalty.Wallet.Application.Command.AppleWallet
{
    public class SendPushNotificationCommand : IRequest<Result<bool>>
    {
        public string CustomerId { get; set; }
        public string TierCode { get; set; }
    }
}
