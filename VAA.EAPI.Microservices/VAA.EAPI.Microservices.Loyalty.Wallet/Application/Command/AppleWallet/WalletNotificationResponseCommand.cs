using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Wallet.Domain;

namespace VAA.EAPI.Microservices.Loyalty.Wallet.Application.Command.AppleWallet
{
    public class WalletNotificationResponseCommand : IRequest<Result<Response<WalletResponse>>>
    {
        public WalletNotificationResponseCommand(string correlationId)
        {
            CorrelationId = correlationId;
        }

        public string CorrelationId { get; set; }

    }
}
