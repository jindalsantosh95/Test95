using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Loyalty.Wallet.Constants;
using VAA.EAPI.Microservices.Loyalty.Wallet.Infrastructure.Airship;
using System;

namespace VAA.EAPI.Microservices.Loyalty.Wallet.Helper
{
    public class AirshipWalletHelper
    {
        public static bool IsAirshipUptoDate(FlightRequest request, FlightResponseFields responseFields)
        {
            if (responseFields == null)
                return true;

            var comparisons = new[]
                {
                    (request.Fields.FlightNumber?.Value,       responseFields.FlightNumber?.Value),
                    (request.Fields.AirlineCode?.Value,        responseFields.AirlineCode?.Value),
                    (request.Fields.AirlineName?.Value,        responseFields.AirlineName?.Value),
                    (request.Fields.DepartureAirport?.Value,   responseFields.DepartureAirport?.Value),
                    (request.Fields.DepartureTime?.Value,      responseFields.DepartureTime?.Value),
                    (request.Fields.ArrivalTime?.Value,        responseFields.ArrivalTime?.Value),
                    (request.Fields.BoardingTime?.Value,       responseFields.BoardingTime?.Value),
                    (request.Fields.ArrivalAirport?.Value,     responseFields.ArrivalAirport?.Value),
                    (request.Fields.DepartureGate?.Value,      responseFields.DepartureGate?.Value),
                    (request.Fields.Semantics?.DepartureTerminal,    responseFields.Semantics?.DepartureTerminal)
                };

            return comparisons.All(pair =>
                (pair.Item1 == null && pair.Item2 == null) ||
                (pair.Item1 != null && pair.Item2 != null && pair.Item1.Is(pair.Item2))
            );
        }

        public static bool HaveBoardingPassesChanged(IEnumerable<FlightRequestModel> requestPayload, IEnumerable<Link> responsePayload)
        {
            if (requestPayload == null || responsePayload == null)
                return true;

            var res = responsePayload
                .SelectMany(l => l.Payload?.Flights ?? [])
                .SelectMany(f => f.Passengers ?? [])
                .ToList();
            foreach (var req in requestPayload.SelectMany(f => f.Passengers))
            {
                var r = res.FirstOrDefault(x =>
                    x.Fields.PassengerName.Value.Is(req.Fields.PassengerName.Value) && x.Fields.BoardingTime.Value.Is(req.Fields.BoardingTime.Value));

                if (r == null)
                    return true;

                bool ok =
                    (req?.Fields?.PassengerName?.Value).Is(r?.Fields?.PassengerName?.Value) &&
                    (req?.Fields?.Barcode_value?.Value).Is(r?.Fields?.Barcode_value?.Value) &&
                    (req?.Fields?.SeatNumber?.Value).Is(r?.Fields?.SeatNumber?.Value) &&
                    (req?.Fields?.Remarks?.Value).Is(r?.Fields?.Remarks?.Value) &&
                    (req?.Fields?.TierName?.Value).Is(r?.Fields?.TierName?.Value) &&
                    (req?.Fields?.TierCode?.Value).Is(r?.Fields?.TierCode?.Value) &&
                    (req?.Fields?.SeatClass?.Value).Is(r?.Fields?.SeatClass?.Value) &&
                    (req?.Fields?.SecurityProgramLogo?.Value).Is(r?.Fields?.SecurityProgramLogo?.Value) &&
                    (req?.Fields?.BoardingPrivilegeImage?.Value).Is(r?.Fields?.BoardingPrivilegeImage?.Value);

                if (!ok)
                {
                    return true;
                }
            }

            return false;
        }

        public static TemplateCode GetTemplateCode(string operatingCarrierCode, string cabinName)
        {
            operatingCarrierCode = operatingCarrierCode?.Trim();

            if (operatingCarrierCode.Is("DL"))
                return TemplateCode.DL;

            if (!operatingCarrierCode.Is("VS"))
                return TemplateCode.OA;

            return cabinName?.Trim().ToUpperInvariant() switch
            {
                "BUSINESS" => TemplateCode.VS_UPPER,
                "UPPER CLASS" => TemplateCode.VS_UPPER,
                "UPPER" => TemplateCode.VS_UPPER,
                "PREMIUM ECONOMY" => TemplateCode.VS_PREMIUM,
                "PREMIUM" => TemplateCode.VS_PREMIUM,
                _ => TemplateCode.VS_ECONOMY
            };
        }

        public static string GetIosTemplateCode(TemplateCode? templateCode)
        {
            return (templateCode) switch
            {
                (TemplateCode.VS_PREMIUM) => ConfigReader.Get(Constants.Environment.AirshipIosPremiumTemplate),
                (TemplateCode.VS_UPPER) => ConfigReader.Get(Constants.Environment.AirshipIosUpperTemplate),
                (TemplateCode.OA) => ConfigReader.Get(Constants.Environment.AirshipIosOATemplate),
                (TemplateCode.DL) => ConfigReader.Get(Constants.Environment.AirshipIosDLTemplate),
                _ => ConfigReader.Get(Constants.Environment.AirshipIosEconomyTemplate)
            };

        }
        public static string GetAndroidTemplateCode(TemplateCode? templateCode)
        {
            return (templateCode) switch
            {
                (TemplateCode.VS_PREMIUM) => ConfigReader.Get(Constants.Environment.AirshipAndroidPremiumTemplate),
                (TemplateCode.VS_UPPER) => ConfigReader.Get(Constants.Environment.AirshipAndroidUpperTemplate),
                (TemplateCode.DL) => ConfigReader.Get(Constants.Environment.AirshipAndroidDLTemplate),
                (TemplateCode.OA) => ConfigReader.Get(Constants.Environment.AirshipAndroidOATemplate),
                _ => ConfigReader.Get(Constants.Environment.AirshipAndroidEconomyTemplate)
            };
        }

        public static string GetSecureProgramImageUrl(bool skyPriority, bool tsaPreCheck) =>
           (skyPriority, tsaPreCheck) switch
           {
               (true, true) => ConfigReader.Get(Constants.Environment.SecureProgramImageUrl),
               (true, false) => ConfigReader.Get(Constants.Environment.SkyPriorityImgUrl),
               (false, true) => ConfigReader.Get(Constants.Environment.TSAPreImgUrl),
               _ => string.Empty
           };

        public static string GetTierName(string tierCode)
        {
            if (tierCode.IsNullOrEmpty())
                return null;

            return tierCode switch
            {
                "VS_Gold" => "GOLD",
                "VS_Silver" => "SILVER",
                "VS_Red" => "RED",
                _ => null
            };
        }

        public static string BuildAdaptivePassUrl(string configuredUrl, IReadOnlyCollection<string> ids, string deviceType = null)
        {
            if (!Uri.TryCreate(configuredUrl?.Trim(), UriKind.Absolute, out var baseUri))
                return null;

            var validEncodedIds = (ids ?? [])
                .Where(x => !x.IsNullOrWhiteSpace())
                .Select(Uri.EscapeDataString)
                .ToArray();

            if (validEncodedIds.Length == 0)
                return null;

            var query = $"ids={string.Join(",", validEncodedIds)}";
            if (!deviceType.IsNullOrWhiteSpace())
                query = $"{query}&deviceType={Uri.EscapeDataString(deviceType)}";

            var uriBuilder = new UriBuilder(baseUri)
            {
                Path = baseUri.AbsolutePath.TrimEnd('/'),
                Query = query,
                Fragment = string.Empty
            };

            return uriBuilder.Uri.AbsoluteUri;
        }
    }
}
