using System.Globalization;
using HotChocolate;
using NodaTime;
using VAA.EAPI.GraphQL.Domain.PaxOps.Constants;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.Enums;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Models.BoardingPass
{
    public class BoardingPass
    {
        public string PnrLocator { get; set; }
        public string GivenNames { get; set; }
        public string Surname { get; set; }
        public string PassengerTypeCode { get; set; }
        public string PassengerNIN { get; set; }
        public string PassengerCustomerId { get; set; }
        public string PassengerId { get; set; }
        public string SeatNumber { get; set; }
        public string Zone { get; set; }
        public string Terminal { get; set; }
        public string OperatingAirlineName { get; set; }
        public string MarketingAirlineName { get; set; }
        public string OperatingFlightNumber { get; set; }
        public string MarketingFlightNumber { get; set; }
        public string DestinationAirportCode { get; set; }
        public string OriginAirportCode { get; set; }
        [GraphQLDeprecated("Use DepartureLocal")]
        public DateTime DepartureDateTime { get; set; }
        public LocalDateTime DepartureLocal => LocalDateTime.FromDateTime(DepartureDateTime);
        public string QrCode { get; set; }
        public string QrCodeData { get; set; }
        public string DesktopBoardingPassBase64 { get; set; }
        public string VerificationStatusMessage { get; set; }
        public TemplateCode TemplateCode { get; set; }
        public string CabinName { get; set; }
        public string Gate { get; set; }
        public string BoardingTime { get => BoardingTimeLocalTime?.ToString("HH:mm", CultureInfo.InvariantCulture) ?? string.Empty; }
        public LocalTime? BoardingTimeLocalTime { get; set; }
        public bool ShowSkyPriority { get; set; }
        public bool ShowTsaPre { get; set; }
        public string FlightStatusCode { get; set; }
        public string InfantName { get; set; }
        public bool CheckedIn { get; set; }
        public string IosBoardingPassWalletLink { get; set; }
        public string AndroidBoardingPassWalletLink { get; set; }
        public string BoardingPassAdaptiveLink { get; set; }
        public LoyaltyTierCode? TierCode { get; set; }
        public string  TierName { get; set; }
        public FlightSegmentStatus? OperationalStatus { get; set; }

        public static explicit operator BoardingPass(Infrastructure.Models.CheckInMS.BoardingPass source)
        {
            return new BoardingPass()
            {
                PnrLocator = source?.PnrLocator,
                GivenNames = source?.GivenNames,
                Surname = source?.Surname,
                PassengerNIN = source?.PassengerNIN,
                PassengerCustomerId = source?.PassengerCustomerId,  
                PassengerId = source?.PassengerId,
                SeatNumber = source?.SeatNumber,
                Zone = source?.Zone,
                Terminal = source?.Terminal,
                MarketingAirlineName = source?.MarketingAirlineName,
                OperatingAirlineName = source?.OperatingAirlineName,
                OperatingFlightNumber = source?.OperatingFlightNumber,
                MarketingFlightNumber = source?.MarketingFlightNumber,
                DestinationAirportCode = source?.DestinationAirportCode,
                OriginAirportCode = source?.OriginAirportCode,
                DepartureDateTime = source?.DepartureDateTime ?? default,
                QrCode = source?.QrCode,
                QrCodeData = source?.QrCodeData,
                DesktopBoardingPassBase64 = source?.DesktopBoardingPassBase64,
                VerificationStatusMessage = source?.VerificationStatusMessage,
                TemplateCode = source?.TemplateCode ?? default,
                CabinName = source?.CabinName,
                Gate = source?.Gate,
                BoardingTimeLocalTime = source?.BoardingTimeLocalTime,
                ShowSkyPriority = source?.ShowSkyPriority ?? false,
                ShowTsaPre = source?.ShowTsaPre ?? false,
                FlightStatusCode = source?.FlightStatusCode.ToString(),
                InfantName = source?.InfantName,
                CheckedIn = source?.CheckedIn ?? false
            };
        }
    }
}
