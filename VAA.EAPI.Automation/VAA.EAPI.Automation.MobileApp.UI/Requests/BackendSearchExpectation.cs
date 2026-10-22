using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.MobileApp.UI.Models;

namespace VAA.EAPI.Automation.MobileApp.UI.Requests;

internal static class BackendSearchExpectation
{
    internal static string SearchPayload(FlightSearch search)
    {
        if (search.IsReturn || search.Passengers != PassengerCounts.OneAdult || search.Origin != "LHR" || search.Destination != "JFK")
            throw new InvalidOperationException("TC41 currently qualifies one adult, one-way LHR-JFK revenue/GBP only.");
        // Same contract as Search automation's OfferSearchBFFRequest.
        return new JObject
        {
            ["slices"] = new JArray(new JObject { ["origin"] = search.Origin, ["destination"] = search.Destination, ["departure"] = search.DepartureDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) }),
            ["awardSearch"] = false,
            ["passengers"] = new JArray("ADULT"),
            ["cabinClasses"] = new JArray()
        }.ToString(Formatting.None);
    }

    internal static (Guid BasketId, string FareId) SelectEconomyFare(string body, bool connectingPartner = false)
    {
        try
        {
            JObject result = JObject.Parse(body);
            foreach (JToken candidate in result.SelectTokens("$..slice.flightsAndFares[*]"))
            {
                JArray? segments = candidate.SelectToken("flight.segments") as JArray;
                if (segments is null || !MatchesItineraryKind(segments, connectingPartner))
                    continue;
                foreach (JToken fare in candidate["fares"] as JArray ?? [])
                {
                    if (!fare.Value<string>("fareFamilyType").Is("ECON"))
                        continue;
                    string link = Required(fare, "links[?(@.rel == 'summary')].href");
                    if (!Uri.TryCreate(link, UriKind.Absolute, out Uri? uri))
                        throw new JsonException();
                    string[] parts = uri.AbsolutePath.Trim('/').Split('/');
                    if (parts.Length < 4 || !Guid.TryParse(parts[^4], out Guid id) || id == Guid.Empty ||
                        parts[^3] != "summary" || parts[^2] != "slice1.1" || parts[^1].IsNullOrWhiteSpace())
                        throw new JsonException();
                    // Extract identity only; never send credentials or requests to response-supplied hosts.
                    return (id, "slice1.1|" + Uri.UnescapeDataString(parts[^1]));
                }
            }
            throw new InvalidOperationException($"Backend search returned no supported {(connectingPartner ? "one-stop partner" : "direct Virgin")} Economy offer; UI search has not started.");
        }
        catch (Exception exception) when (exception is JsonException or ArgumentException or FormatException or InvalidCastException)
        {
            throw new InvalidOperationException("Backend search response did not satisfy the expected fare contract; raw response suppressed.");
        }
    }

    internal static BackendFareExpectation ReadSummary(string body, FlightSearch search, bool connectingPartner = false)
    {
        try
        {
            JObject summary = JObject.Parse(body);
            JArray flights = summary["flights"] as JArray ?? throw new JsonException();
            if (flights.Count != 1)
                throw new JsonException();
            JToken flight = flights[0];
            JArray segments = flight["segments"] as JArray ?? throw new JsonException();
            if (!MatchesItineraryKind(segments, connectingPartner))
                throw new JsonException();
            List<FlightLegDetails> legs = [];
            List<string> flightNumbers = [];
            foreach (JToken segment in segments)
            {
                DateTime departure = DateTime.Parse(Required(segment, "departure"), CultureInfo.InvariantCulture);
                DateTime arrival = DateTime.Parse(Required(segment, "arrival"), CultureInfo.InvariantCulture);
                legs.Add(new FlightLegDetails(Required(segment, "origin.code"), Required(segment, "destination.code"),
                    DateOnly.FromDateTime(departure), DateOnly.FromDateTime(arrival), TimeOnly.FromDateTime(departure),
                    TimeOnly.FromDateTime(arrival), Required(segment, "operatingAirline.name"), null));
                flightNumbers.Add(Required(segment, "flightNumber"));
            }
            if (legs[0].Origin != search.Origin || legs[^1].Destination != search.Destination || legs[0].DepartureDate != search.DepartureDate ||
                legs.Zip(legs.Skip(1)).Any(pair => pair.First.Destination != pair.Second.Origin) || !Required(summary, "price.currency").Is("GBP") ||
                !Required(flight, "fareFamilyType").Is("ECON"))
                throw new JsonException();
            return new BackendFareExpectation(legs, flightNumbers, Required(flight, "cabinName"), "GBP", BasketBaggageSnapshot.Read(body));
        }
        catch (Exception exception) when (exception is JsonException or ArgumentException or FormatException or InvalidCastException)
        {
            throw new InvalidOperationException("Backend summary must match the requested itinerary type, continuous route, Economy journey and GBP currency; raw response suppressed.");
        }
    }

    private static string Required(JToken token, string path)
        => token.SelectToken(path)?.Value<string>() is string value && !value.IsNullOrWhiteSpace() ? value : throw new JsonException();

    private static bool MatchesItineraryKind(JArray segments, bool connectingPartner)
    {
        if (segments.Count != (connectingPartner ? 2 : 1))
            return false;
        string[] carriers = segments.Select(segment => segment.SelectToken("operatingAirline.code")?.Value<string>() ?? string.Empty).ToArray();
        if (carriers.Any(carrier => carrier.IsNullOrWhiteSpace()))
            return false;
        return connectingPartner
            ? carriers.Any(carrier => !carrier.Is("VS"))
            : carriers[0].Is("VS") && segments[0].Value<int?>("stopCount") == 0;
    }
}
