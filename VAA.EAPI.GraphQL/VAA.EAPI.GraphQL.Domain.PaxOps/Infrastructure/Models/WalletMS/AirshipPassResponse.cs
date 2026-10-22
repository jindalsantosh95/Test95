using NodaTime;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.WalletMS
{
    public class AirshipPassResponse
    {
        public string BoardingPassAdaptiveLink { get; set; }
        public string IosBoardingPassWalletLink { get; set; }
        public string AndroidBoardingPassWalletLink { get; set; }
        public string PassengerNIN { get; set; }
        public string OriginAirportCode { get; set; }
        public LocalDateTime? DepartureDateLocalDateTime { get; set; }
    }
}
