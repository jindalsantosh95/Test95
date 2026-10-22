using Newtonsoft.Json.Converters;
using NodaTime;
using VAA.EAPI.GraphQL.Domain.PaxOps.Constants;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.WalletMS
{
    public class WalletBoardingPassRequest
    {
        public string PnrLocator { get; set; }
        public string GivenNames { get; set; }
        public string Surname { get; set; }
        public string PassengerNIN { get; set; }
        public string SeatNumber { get; set; }
        public string Zone { get; set; }
        public string Terminal { get; set; }
        public string OperatingFlightNumber { get; set; }
        public string OriginAirportCode { get; set; }
        public LocalDateTime DepartureLocal { get; set; }
        public string QrCodeData { get; set; }
        public string VerificationStatusMessage { get; set; }
        [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
        public TemplateCode TemplateCode { get; set; }
        public string CabinName { get; set; }
        public string Gate { get; set; }
        public LocalTime? BoardingTimeLocalTime { get; set; }
        public bool ShowSkyPriority { get; set; }
        public bool ShowTsaPre { get; set; }
        public bool CheckedIn { get; set; }
        public string TierCode { get; set; }
        public string TierName { get; set; }

        public static explicit operator WalletBoardingPassRequest(VAA.EAPI.GraphQL.Domain.PaxOps.Models.BoardingPass.BoardingPass boardingPass)
        {
            return new WalletBoardingPassRequest
            {
                PnrLocator = boardingPass?.PnrLocator,
                GivenNames = boardingPass?.GivenNames,
                Surname = boardingPass?.Surname,
                PassengerNIN = boardingPass?.PassengerNIN,
                SeatNumber = boardingPass?.SeatNumber,
                Zone = boardingPass?.Zone,
                Terminal = boardingPass?.Terminal,
                OperatingFlightNumber = boardingPass?.OperatingFlightNumber,
                OriginAirportCode = boardingPass?.OriginAirportCode,
                DepartureLocal = boardingPass == null ? default : boardingPass.DepartureLocal,
                QrCodeData = boardingPass?.QrCodeData,
                VerificationStatusMessage = boardingPass?.VerificationStatusMessage,
                TemplateCode = boardingPass?.TemplateCode ?? default,
                CabinName = boardingPass?.CabinName,
                Gate = boardingPass?.Gate,
                BoardingTimeLocalTime = boardingPass?.BoardingTimeLocalTime,
                ShowSkyPriority = boardingPass?.ShowSkyPriority ?? false,
                ShowTsaPre = boardingPass?.ShowTsaPre ?? false,
                CheckedIn = boardingPass?.CheckedIn ?? false,
                TierCode = boardingPass?.TierCode.HasValue == true ? boardingPass.TierCode.Value.ToString() : null,
                TierName = boardingPass?.TierName
            };
        }
    }
}
