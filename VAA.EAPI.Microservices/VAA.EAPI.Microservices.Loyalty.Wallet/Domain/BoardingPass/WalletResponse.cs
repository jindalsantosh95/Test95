using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Loyalty.Wallet.Domain.BoardingPass
{
    public class WalletResponse
    {
        public IEnumerable<WalletPassResponse> WalletPasses { get; set; }
        public string IosPnrMultiBoardingPassUrl { get; set; }
        public string AndroidPnrMultiBoardingPassUrl { get; set; }
        public string PnrMultiBoardingPassUrl { get; set; }
    }
}
