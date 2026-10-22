using NodaTime;
using VAA.EAPI.Microservices.Loyalty.Wallet.Infrastructure.Airship;

namespace VAA.EAPI.Microservices.Loyalty.Wallet.Domain.BoardingPass
{
    public class WalletPassResponse
    {
        public string BoardingPassAdaptiveLink { get; set; }
        public string IosBoardingPassWalletLink { get; set; }
        public string AndroidBoardingPassWalletLink { get; set; }
        public string PassengerNIN { get; set; }
        public string OriginAirportCode { get; set; }
        public LocalDateTime? DepartureDateLocalDateTime { get; set; }

        public static explicit operator WalletPassResponse((Link airshipLinkResponse, BoardingPass pass) source)
        {
            return new WalletPassResponse
            {
                BoardingPassAdaptiveLink = source.airshipLinkResponse?.Url,
                IosBoardingPassWalletLink = source.airshipLinkResponse?.IosUrl,
                AndroidBoardingPassWalletLink = source.airshipLinkResponse?.AndroidUrl,
                PassengerNIN = source.pass?.PassengerNIN,
                OriginAirportCode = source.pass?.OriginAirportCode,
                DepartureDateLocalDateTime = source.pass?.DepartureLocal
            };
        }
    }
}
