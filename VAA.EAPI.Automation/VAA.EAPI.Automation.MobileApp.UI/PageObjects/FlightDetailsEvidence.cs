using System.Globalization;
using System.Xml.Linq;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Text;
using VAA.EAPI.Automation.MobileApp.UI.Models;

namespace VAA.EAPI.Automation.MobileApp.UI.PageObjects;

internal static class FlightDetailsEvidence
{
    internal static readonly string[] RequiredFields =
        ["OriginAirportCode", "DestinationAirportCode", "DepartureDate", "ArrivalDate",
         "DepartureTime", "ArrivalTime", "OperatingCarrier"];

    internal static IReadOnlyList<string> RequiredIds(int stops)
    {
        if (stops is < 0 or > 3)
            throw new InvalidOperationException("Flight details qualification supports zero to three connections.");
        return Enumerable.Range(1, stops + 1)
            .SelectMany(index => RequiredFields.Select(field => NativeSearchAccessibilityIds.FlightLegField(index, field)))
            .Concat(Enumerable.Range(1, stops).Select(NativeSearchAccessibilityIds.FlightConnection)).ToArray();
    }

    internal static void Capture(string source, int stops, IDictionary<string, string> observed)
    {
        HashSet<string> expected = RequiredIds(stops).Concat(Enumerable.Range(1, stops + 1)
            .Select(index => NativeSearchAccessibilityIds.FlightLegField(index, "Descriptor"))).ToHashSet(StringComparer.Ordinal);
        foreach (IGrouping<string, XElement> group in XDocument.Parse(source).Descendants()
            .Where(element => expected.Contains((string?)element.Attribute("name") ?? string.Empty))
            .GroupBy(element => (string)element.Attribute("name")!, StringComparer.Ordinal))
        {
            bool valueRequired = group.Key.EndsWith("AirportCode", StringComparison.Ordinal) ||
                group.Key.EndsWith("Date", StringComparison.Ordinal) ||
                group.Key.StartsWith(NativeSearchAccessibilityIds.FlightConnectionPrefix, StringComparison.Ordinal);
            string[] values = group.Select(element => valueRequired
                    ? (string?)element.Attribute("value")
                    : (string?)element.Attribute("label") ?? (string?)element.Attribute("value"))
                .Where(value => !value.IsNullOrWhiteSpace()).Select(value => value!.Trim())
                .Distinct(StringComparer.Ordinal).ToArray();
            if (values.Length > 1)
                throw new InvalidOperationException($"Ambiguous Flight details evidence for '{group.Key}'.");
            if (values.Length == 0)
                continue;
            if (observed.TryGetValue(group.Key, out string? previous) && previous != values[0])
                throw new InvalidOperationException($"Flight details changed while reading '{group.Key}'.");
            observed[group.Key] = values[0];
        }
    }

    internal static ConnectingFlightDetails Read(IReadOnlyDictionary<string, string> values, int stops)
    {
        foreach (string id in RequiredIds(stops))
            if (!values.ContainsKey(id))
                throw new InvalidOperationException($"Missing Flight details value: '{id}'.");
        FlightLegDetails[] legs = Enumerable.Range(1, stops + 1).Select(index =>
        {
            string Field(string field) => values[NativeSearchAccessibilityIds.FlightLegField(index, field)];
            return new FlightLegDetails(Airport(Field("OriginAirportCode")), Airport(Field("DestinationAirportCode")),
                DateOnly.ParseExact(Field("DepartureDate"), "yyyy-MM-dd", CultureInfo.InvariantCulture),
                DateOnly.ParseExact(Field("ArrivalDate"), "yyyy-MM-dd", CultureInfo.InvariantCulture),
                Clock(Field("DepartureTime")), Clock(Field("ArrivalTime")), Field("OperatingCarrier"),
                values.GetValueOrDefault(NativeSearchAccessibilityIds.FlightLegField(index, "Descriptor")));
        }).ToArray();
        return new ConnectingFlightDetails(legs,
            Enumerable.Range(1, stops).Select(index => Airport(values[NativeSearchAccessibilityIds.FlightConnection(index)])).ToArray());
    }

    private static string Airport(string value)
        => RegexCompiled.IsMatch(value, "^[A-Z]{3}$") ? value
            : throw new InvalidOperationException("Flight details airport value must be an IATA code.");

    private static TimeOnly Clock(string label)
    {
        string[] matches = RegexCompiled.Matches(label, @"\b\d{1,2}:\d{2}\b").Select(match => match.Value).ToArray();
        if (matches.Length != 1)
            throw new InvalidOperationException("Flight details must expose one clock time per directional field.");
        return TimeOnly.ParseExact(matches[0], ["H:mm", "HH:mm"], CultureInfo.InvariantCulture);
    }
}
