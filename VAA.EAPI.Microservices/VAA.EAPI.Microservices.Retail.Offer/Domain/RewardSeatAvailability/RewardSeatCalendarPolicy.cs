using System;
using System.Linq;
using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Microservices.Retail.Offer.Domain.RewardSeatAvailability
{
    // Server-owned controls are introduced only when the ITA request is built.
    public sealed class RewardSeatCalendarPolicy
    {
        public string PermittedCarrier { get; init; } = "VS";
        public string[] PassengerTypes { get; set; } = ["ADT"];
        public string Market { get; init; } = "GB";
        public required string MarketPolicyVersion { get; init; }
        public required string CountryIssued { get; init; }
        public required string CountrySold { get; init; }
        public required string Currency { get; init; }
        public required string SalesCity { get; init; }

        public static RewardSeatCalendarPolicy Load()
        {
            var prefix = $"{Constants.Environment.RewardSeatAvailabilityMarkets}:GB:";
            var policy = new RewardSeatCalendarPolicy
            {
                MarketPolicyVersion = ConfigReader.Get(Constants.Environment.RewardSeatAvailabilityMarketPolicyVersion).Trim(),
                CountryIssued = ConfigReader.Try(prefix + Constants.Environment.RewardSeatMarketCountryIssued)?.Trim() ?? string.Empty,
                CountrySold = ConfigReader.Try(prefix + Constants.Environment.RewardSeatMarketCountrySold)?.Trim() ?? string.Empty,
                Currency = ConfigReader.Try(prefix + Constants.Environment.RewardSeatMarketCurrency)?.Trim() ?? string.Empty,
                SalesCity = ConfigReader.Try(prefix + Constants.Environment.RewardSeatMarketSalesCity)?.Trim() ?? string.Empty
            };
            if (policy.MarketPolicyVersion.Length == 0 ||
                !IsCode(policy.CountryIssued) || !IsCode(policy.CountrySold) ||
                !IsCode(policy.Currency) || !IsCode(policy.SalesCity))
                throw new InvalidOperationException("Reward calendar GB market configuration is invalid.");
            return policy;
        }

        private static bool IsCode(string value) =>
            value is { Length: 3 } && value.All(char.IsAsciiLetterUpper);
    }
}
