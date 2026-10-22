using VAA.EAPI.Common.Extensions;

namespace VAA.EAPI.Microservices.Retail.Holidays.Domain.Trips
{
    public class AirportDetails
    {
        public string? TerminalName { get; set; }

        public string Code { get; set; } = default!;

        public string? AirportName { get; set; }
        public string? CityName { get; set; }
        public string? CountryName { get; set; }
        public string? CountryCode { get; set; }

        public static explicit operator AirportDetails?(string? source)
        {
            if (source == null)
                return null;

            return new AirportDetails
            {
                Code = source
            };
        }

        public static explicit operator AirportDetails?((string? code, string? terminal) source)
        {
            var (code, terminal) = source;
            if (code == null)
                return null;

            return new AirportDetails
            {
                Code = code,
                TerminalName = FormatTerminalName(terminal)
            };
        }

        private static string? FormatTerminalName(string? terminal)
        {
            if (terminal.IsNullOrWhiteSpace())
                return null;

            var trimmed = terminal.Trim();
            if (trimmed.Is(null))
                return null;

            if (trimmed.StartsWith("Terminal", System.StringComparison.OrdinalIgnoreCase))
                return trimmed;

            return trimmed.Length <= 3 ? $"Terminal {trimmed}" : trimmed;
        }
    }
}