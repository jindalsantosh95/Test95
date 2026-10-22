using System;
using System.Globalization;
using System.Linq;

namespace VAA.EAPI.Orchestration.Notifications.Domain.Entities
{
    public class AircraftMessage
    {
        public string Id => $"{FlightDate:yyyyMMdd}{FlightNumber}";
        public string MessageType { get; set; }
        public string Timezone { get; set; }
        public string Category { get; set; }
        public string FlightNumber { get; set; }
        public DateTime FlightDate { get; set; }
        public string Route { get; set; }
        public string Origin { get; set; }
        public string Destination { get; set; }
        public string ReturnInfo { get; set; }
        public string CabinClass { get; set; }
        public string AircraftCode { get; set; }
        public string SeatConfig { get; set; }
        public string AircraftRegistration { get; set; }
        public string AdditionalInfo { get; set; }


        public static explicit operator AircraftMessage(string rawMessage)
        {
            if (string.IsNullOrWhiteSpace(rawMessage)) throw new FormatException("Invalid data format: message payload is empty");

            var lines = rawMessage.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            if (!(lines.Length > 4)) throw new FormatException($"Invalid data format: {rawMessage}");

            var model = new AircraftMessage
            {
                MessageType = lines[0].Trim(),
                Timezone = lines[1].Trim(),
                Category = lines[2].Trim()
            };

            if (model.Category != "EQT")
                return model;
            // ---- Line 4: Flight Details ----
            // VS110/28MAY26 ATL/MAN 6/VS127/29
            var flightParts = lines[3].Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);


            if (flightParts.Length < 2)
                throw new FormatException($"Invalid flight details: {lines[3]}");

            var flightInfo = flightParts[0].Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            model.FlightNumber = flightInfo[0].Replace("vs", "", StringComparison.OrdinalIgnoreCase);

            if (flightInfo.Length > 1)
                model.FlightDate = ParseDate(flightInfo[1]);

            model.Route = flightParts[1];

            var routeParts = model.Route.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (routeParts.Length < 2)
                throw new FormatException($"Invalid route format: {model.Route}");

            model.Origin = routeParts.First();
            model.Destination = routeParts.Last();

            // Optional return info
            if (flightParts.Length > 2)
                model.ReturnInfo = string.Join(" ", flightParts.Skip(2));

            // ---- Line 5: Cabin + Aircraft ----
            // J 333 J31W48Y185.J31W48Y185 G-VUFO
            var seatParts = lines[4].Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (seatParts.Length < 3)
                throw new FormatException($"Invalid cabin/seat: {lines[4]}");

            model.CabinClass = seatParts[0];

            model.AircraftCode = seatParts[1];

            model.SeatConfig = seatParts[2];

            if (seatParts.Length > 3)
                model.AircraftRegistration = seatParts[3];

            // ---- Line 6: Additional info ----
            if (lines.Length > 5)
                model.AdditionalInfo = lines[5];

            return model;
        }

        private static DateTime ParseDate(string value)
        {
            // Example: 28MAY26
            return DateTime.TryParseExact(value, "ddMMMyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) ? date : throw new FormatException($"Invalid date format: {value}");
        }
    }
}
