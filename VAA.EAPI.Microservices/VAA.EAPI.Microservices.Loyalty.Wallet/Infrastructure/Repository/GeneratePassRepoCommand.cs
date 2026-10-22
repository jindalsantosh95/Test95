using MediatR;

namespace VAA.EAPI.Microservices.Loyalty.Wallet.Infrastructure.Repository
{
    public class GeneratePassRepoCommand : IRequest<byte[]>
    {
        public string FcNumber { get; set; }
        public string MemberName { get; set; }
        public string TierCode { get; set; }
        public string ValidUntil { get; set; }
    }

}
