using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.Microservices.Loyalty.Wallet.Domain
{
    public class WalletResponse
    {
        public string Value { get; set; }
        public IEnumerable<ErrorDetails> Errors { get; set; }
    }
}
