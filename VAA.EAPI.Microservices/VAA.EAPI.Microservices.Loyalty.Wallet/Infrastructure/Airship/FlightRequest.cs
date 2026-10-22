using System;
using System.Globalization;
using System.Runtime.InteropServices;
using VAA.EAPI.Microservices.Loyalty.Wallet.Domain.BoardingPass;

namespace VAA.EAPI.Microservices.Loyalty.Wallet.Infrastructure.Airship
{
    public class FlightRequest
    {
        public FlightRequestFields Fields { get; set; }

        public static explicit operator FlightRequest((Segment segment, BoardingPass boardingPass, AirportDetails originAirport, AirportDetails destinationAirport) source)
        {
            var operatingCarrierCode = source.segment?.OperatingCarrier?.CarrierCode?.Trim();
            var boardingDateTime = source.segment.ScheduledDepartureLocalDateTime.HasValue && source.boardingPass?.BoardingTimeLocalTime.HasValue == true ? (source.segment.ScheduledDepartureLocalDateTime.Value.Date + source.boardingPass.BoardingTimeLocalTime.Value).ToString(Constants.Application.IsoLocalDateTimeFormat, null) : string.Empty;

            var departureDateTime = source.segment.ScheduledDepartureLocalDateTime.HasValue ? source.segment.ScheduledDepartureLocalDateTime.Value.ToString(Constants.Application.IsoLocalDateTimeFormat, null) : string.Empty;

            var arrivalDateTime = source.segment.ScheduledArrivalLocalDateTime.HasValue ? source.segment.ScheduledArrivalLocalDateTime.Value.ToString(Constants.Application.IsoLocalDateTimeFormat, null) : string.Empty;

            return new FlightRequest
            {
                Fields = new FlightRequestFields
                {
                    FlightNumber = new FlightNumber
                    {
                        Value = source.segment.OperatingCarrier.FlightNum
                    },
                    AirlineCode = new AirlineCode
                    {
                        Value = operatingCarrierCode
                    },
                    AirlineName = new AirlineName
                    {
                        Value = source.segment.OperatingCarrier.CarrierName
                    },
                    DepartureAirport = new Airport
                    {
                        Label = source.segment.Origin.AirportName,
                        Value = source.segment.Origin.Code,
                    },
                    DepartureGate = new Airport
                    {
                        Label = "Gate",
                        Value = source.boardingPass?.Gate ?? string.Empty,
                    },
                    BoardingTime = new FlightSchedule
                    {
                        Value = boardingDateTime
                    },
                    DepartureTime = new FlightSchedule
                    {
                        Value = departureDateTime
                    },
                    ArrivalAirport = new Airport
                    {
                        Label = source.segment.Destination.AirportName,
                        Value = source.segment.Destination.Code,
                    },
                    ArrivalTime = new FlightSchedule
                    {
                        Value = arrivalDateTime
                    },
                    
                    Semantics = new FlightSemantic
                    {
                        AirlineCode = operatingCarrierCode,
                        FlightNumber = source.segment?.OperatingCarrier?.FlightNum,
                        OriginalBoardingDate = ConvertWithOffset(boardingDateTime, source.originAirport?.TimeZone),
                        OriginalDepartureDate = ConvertWithOffset(departureDateTime, source.originAirport?.TimeZone),
                        OriginalArrivalDate = ConvertWithOffset(arrivalDateTime, source.destinationAirport?.TimeZone),
                        DepartureAirportCode = source.segment?.Origin?.Code,
                        DepartureLocationTimeZone = source.originAirport?.TimeZone,
                        DestinationAirportCode = source.segment?.Destination?.Code,
                        DestinationLocationTimeZone = source.destinationAirport?.TimeZone,
                        DepartureCityName = source.originAirport?.CityName,
                        DestinationCityName = source.destinationAirport?.CityName,
                        DepartureGate = source.boardingPass?.Gate ?? string.Empty,
                        DepartureTerminal = source.boardingPass?.Terminal?.Replace("Terminal", "", StringComparison.OrdinalIgnoreCase)?.Trim() ?? string.Empty,
                        DepartureLocationSecurityPrograms = ["tsaPreCheck"]
                    }
                }
            };
        }

        private static string ConvertWithOffset(string dateTime, string timeZoneId)
        {
            if (string.IsNullOrWhiteSpace(dateTime) || string.IsNullOrWhiteSpace(timeZoneId))
                return dateTime;

            if (!DateTime.TryParseExact(
                    dateTime,
                    Constants.Application.IsoLocalDateTimeFormat,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var localTime))
                return dateTime;

            var timeZone = ResolveTimeZone(timeZoneId);
            if (timeZone == null)
                return dateTime;

            var offset = timeZone.GetUtcOffset(localTime);

            return new DateTimeOffset(localTime, offset)
                .ToString("yyyy-MM-dd'T'HH:mm:sszzz");
        }

        private static TimeZoneInfo? ResolveTimeZone(string timeZoneId)
        {
            if (string.IsNullOrWhiteSpace(timeZoneId))
                return null;

            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            }
            catch
            {
                // ignore and try conversion fallback
            }

            bool isWindows =
                RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

            string convertedId;

            if (isWindows)
            {
                TimeZoneInfo.TryConvertIanaIdToWindowsId(timeZoneId, out convertedId);
            }
            else
            {
                TimeZoneInfo.TryConvertWindowsIdToIanaId(timeZoneId, out convertedId);
            }

            if (!string.IsNullOrWhiteSpace(convertedId))
            {
                try
                {
                    return TimeZoneInfo.FindSystemTimeZoneById(convertedId);
                }
                catch
                {
                    // ignore
                }
            }

            return null;
        }
    }
}
