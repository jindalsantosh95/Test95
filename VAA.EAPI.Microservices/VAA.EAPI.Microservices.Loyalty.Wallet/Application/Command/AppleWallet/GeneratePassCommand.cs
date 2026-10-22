using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.Microservices.Loyalty.Wallet.Application.Command.AppleWallet
{
    public class GeneratePassCommand : IRequest<Response<byte[]>>
    {
        public string FcNumber { get; set; }
        public string MemberName { get; set; }
        public string TierCode { get; set; }
        public string ValidUntil { get; set; }
    }
}
