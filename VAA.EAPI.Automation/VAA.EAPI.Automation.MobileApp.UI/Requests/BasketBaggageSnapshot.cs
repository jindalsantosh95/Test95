using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.MobileApp.UI.Models;

namespace VAA.EAPI.Automation.MobileApp.UI.Requests;

internal static class BasketBaggageSnapshot
{
    internal static IReadOnlyList<BackendBaggageAllowance> Read(string body)
    {
        try
        {
            JObject summary = JObject.Parse(body);
            JArray flights = summary["flights"] as JArray ?? throw new JsonException();
            if (flights.Count != 1)
                throw new JsonException();
            string flightId = flights[0].Value<string>("flightId") ?? string.Empty;
            JArray baggage = summary["flightPassengerBaggage"] as JArray ?? throw new JsonException();
            if (flightId.IsNullOrWhiteSpace() || baggage.Count != 1 ||
                !baggage[0].Value<string>("flightRefId").Is(flightId))
                throw new JsonException();
            List<BackendBaggageAllowance> allowances = [];
            foreach ((string property, string category) in new[] { ("carryOnBag", "carryOn"), ("checkedBag", "checked") })
            {
                JToken? free = baggage[0].SelectToken($"{property}.passengerBaggageAllowance.freeBaggageAllowance");
                // GraphQL/iOS omits an absent free allowance. Do not invent a zero row.
                if (free is null || free.Type == JTokenType.Null)
                    continue;
                int count = free.Value<int?>("allowedBagCnt") ?? throw new JsonException();
                if (count < 0)
                    throw new JsonException();
                JToken? weight = free["weight"];
                string? weightValue = weight is null || weight.Type == JTokenType.Null ? null :
                    Number(weight.Value<string>("unitOfMeasureCnt")) + Unit(weight.Value<string>("unitOfMeasure"), true);
                JToken? dimensions = free["dimensionAllowance"];
                string? dimensionValue = null;
                if (dimensions is not null && dimensions.Type != JTokenType.Null)
                {
                    string?[] axes = [dimensions.Value<string>("height"), dimensions.Value<string>("width"), dimensions.Value<string>("length")];
                    string? linear = dimensions.Value<string>("linear");
                    if (axes.All(axis => !axis.IsNullOrWhiteSpace()))
                        dimensionValue = string.Join("x", axes.Select(Number)) + Unit(dimensions.Value<string>("unitOfMeasure"), false);
                    else if (!linear.IsNullOrWhiteSpace())
                        dimensionValue = Number(linear) + Unit(dimensions.Value<string>("unitOfMeasure"), false);
                    else if (axes.Any(axis => !axis.IsNullOrWhiteSpace()))
                        throw new JsonException();
                }
                allowances.Add(new BackendBaggageAllowance(category, count, weightValue, dimensionValue));
            }
            if (allowances.Count == 0)
                throw new JsonException();
            return allowances;
        }
        catch (Exception exception) when (exception is JsonException or ArgumentException or FormatException or InvalidCastException or OverflowException or InvalidOperationException)
        {
            throw new InvalidOperationException("Backend baggage contract failed: require one summary itinerary, matching baggage flight reference and valid measurements. Raw response suppressed.");
        }
    }

    private static string Number(string? text)
    {
        if (!decimal.TryParse(text, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal value) || value < 0)
            throw new JsonException();
        return value.ToString("G29", CultureInfo.InvariantCulture);
    }

    private static string Unit(string? text, bool weight)
    {
        string unit = (text ?? string.Empty).ToLowerInvariant();
        if (weight && unit == "lbs")
            unit = "lb";
        if (!(weight ? new[] { "kg", "lb" } : new[] { "cm", "in" }).Contains(unit))
            throw new JsonException();
        return unit;
    }
}
