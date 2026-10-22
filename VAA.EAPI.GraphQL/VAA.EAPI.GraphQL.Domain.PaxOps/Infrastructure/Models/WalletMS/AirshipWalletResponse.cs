using System.Collections.Generic;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.WalletMS
{
    public class AirshipWalletResponse
    {
        public IEnumerable<AirshipPassResponse> WalletPasses { get; set; }
        public string? IosPnrMultiBoardingPassUrl { get; set; }
        public string? AndroidPnrMultiBoardingPassUrl { get; set; }
        public string? PnrMultiBoardingPassUrl { get; set; }
    }
}
