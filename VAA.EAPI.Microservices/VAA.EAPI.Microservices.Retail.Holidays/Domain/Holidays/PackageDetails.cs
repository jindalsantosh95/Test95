using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace VAA.EAPI.Microservices.Retail.Holidays.Domain.Holidays
{
    public class PackageDetails
    {
        public string AgentReference { get; set; }
        public DateOnly? DefiniteDueDate { get; set; }
        public DateOnly? DefiniteDate { get; set; }
        public DateOnly? FirmDueDate { get; set; }
        public DateOnly? FirmDate { get; set; }
        public DateOnly? BalanceDueDate { get; set; }
        public Trips.Location Destination { get; set; }

        public static explicit operator PackageDetails((JToken detail, JArray combinedProducts) source)
        {
            var (detail, combinedProducts) = source;

            if (detail == null)
                return null;

            return new()
            {
                AgentReference = detail["agentRef"]?.Value<string>(),
                BalanceDueDate = DateOnly.TryParse(detail["balanceDueDate"]?.Value<string>(), out var balanceDueDate) ? balanceDueDate : null,
                DefiniteDate = DateOnly.TryParse(detail["definiteDate"]?.Value<string>(), out var definiteDate) ? definiteDate : null,
                DefiniteDueDate = DateOnly.TryParse(detail["definiteDueDate"]?.Value<string>(), out var definiteDueDate) ? definiteDueDate : null,
                FirmDate = DateOnly.TryParse(detail["firmDate"]?.Value<string>(), out var firmDate) ? firmDate : null,
                FirmDueDate = DateOnly.TryParse(detail["firmDueDate"]?.Value<string>(), out var firmDueDate) ? firmDueDate : null,
                Destination = HasAdvanceBooking(combinedProducts) ? GetAdvanceBookingLocation(combinedProducts) : (Trips.Location)detail["destination"]
            };
        }

        private static bool HasAdvanceBooking(JArray combinedProducts) =>
              combinedProducts?.OfType<JObject>()
              .Any(p => string.Equals(p["detail"]?["elementGrp"]?.Value<string>(), "ADVANCE", StringComparison.OrdinalIgnoreCase))
              ?? false;

        // Determine destination from the first HTL product that contains a destination; if not available, fall back to first CAR product with a destination
        private static Trips.Location GetAdvanceBookingLocation(JArray combinedProducts)
        {
            Trips.Location computedDestination = null;

            var htlDestinationToken = combinedProducts?.OfType<JObject>()
                .Where(p => string.Equals(p["productCode"]?.Value<string>(), "HTL", StringComparison.OrdinalIgnoreCase))
                .Select(p => p["detail"]?["destination"])
                .FirstOrDefault(d => d != null);

            if (htlDestinationToken == null)
            {
                var carDestinationToken = combinedProducts?.OfType<JObject>()
                    .Where(p => string.Equals(p["productCode"]?.Value<string>(), "CAR", StringComparison.OrdinalIgnoreCase))
                    .Select(p => p["detail"]?["destination"])
                    .FirstOrDefault(d => d != null);

                if (carDestinationToken != null)
                {
                    computedDestination = (Trips.Location)carDestinationToken;
                }
            }
            else
            {
                computedDestination = (Trips.Location)htlDestinationToken;
            }

            return computedDestination;
        }
    }
}