using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.MVT;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Helper
{
    public static class MVTAAMessageParser
    {
        public static bool TryParse(string payload, out MVTMessageRequest request)
        {
            request = null!;

            if (payload.IsNullOrWhiteSpace())
            {
                return false;
            }

            var lines = payload
                .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .ToArray();

            // Line 1 must be MVT
            if (lines.Length < 3 ||
                !lines[0].Is("MVT"))
            {
                return false;
            }

            // Line 3 must start with AA
            if (!lines[2].StartsWith("AA"))
            {
                return false;
            }

            var flightLine = lines[1];

            var slashParts = flightLine.Split('/');

            if (slashParts.Length != 2)
            {
                return false;
            }

            var flightIdentifier = slashParts[0];

            // First 2 letters + 1-4 digits
            if (!RegexCompiled.IsMatch(flightIdentifier, @"^[A-Z]{2}\d{1,4}$"))
            {
                return false;
            }

            var carrier = flightIdentifier[..2];
            var flightNumberString = flightIdentifier[2..];

            if (!int.TryParse(flightNumberString, out var flightNumber))
            {
                return false;
            }

            var routeInfo = slashParts[1];

            if (routeInfo.IsNullOrWhiteSpace())
            {
                return false;
            }

            var routeParts = routeInfo.Split('.', StringSplitOptions.None);

            string arrivalAirport = string.Empty;
            string departureDateValue = string.Empty;

            // Arrival Airport is always the last non-empty segment
            var arrivalAirportPart = routeParts.LastOrDefault(x => !x.IsNullOrWhiteSpace());

            if (!arrivalAirportPart.IsNullOrWhiteSpace())
            {
                arrivalAirport = arrivalAirportPart.Trim();
            }

            if (!RegexCompiled.IsMatch(arrivalAirport, @"^[A-Z]{3}$"))
            {
                return false;
            }

            if (routeParts.Length > 0 && int.TryParse(routeParts[0], out var departureDay))
            {
                var currentDate = DateTime.UtcNow.Date;
                var yesterday = currentDate.AddDays(-1);
                int departureYear;
                int departureMonth;

                DateTime departureDate;

                if (departureDay > currentDate.Day)
                {
                    var previousMonth = currentDate.AddMonths(-1);
                    departureYear = previousMonth.Year;
                    departureMonth = previousMonth.Month;

                }
                else
                {
                    departureYear = currentDate.Year;
                    departureMonth = currentDate.Month;
                }

                if (departureDay < 1 || departureDay > DateTime.DaysInMonth(departureYear, departureMonth))
                {
                    return false;
                }

                departureDate = new DateTime(departureYear, departureMonth, departureDay);

                if (departureDate < yesterday || departureDate > currentDate)
                {
                    return false;
                }

                departureDateValue = departureDate.ToString("yyyy-MM-dd");
            }

            request = new MVTMessageRequest
            {
                FlightCarrier = carrier,
                FlightNumber = flightNumber,
                ArrivalAirport = arrivalAirport,
                DepartureDate = departureDateValue
            };

            // Mandatory fields
            if (request.FlightCarrier.IsNullOrWhiteSpace() || request.ArrivalAirport.IsNullOrWhiteSpace() || request.DepartureDate.IsNullOrWhiteSpace())
            {
                return false;
            }

            return true;
        }
    }
}