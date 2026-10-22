using VAA.EAPI.Automation.Core.Extensions;

namespace VAA.EAPI.Automation.SearchAndBook.UI.Support;

public sealed record SearchSliceDeepLink
{
    public string Origin { get; init; } = "LHR";

    public string Destination { get; init; } = "JFK";

    public DateOnly DepartureDate { get; init; } = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddMonths(6));

    public DateOnly? ReturnDate { get; init; }

    public int Adults { get; init; } = 1;

    public int Teens { get; init; }

    public int Children { get; init; }

    public int Infants { get; init; }

    public bool AwardSearch { get; init; }

    public string? Cta { get; init; }

    public string ToRelativeUrl()
    {
        List<KeyValuePair<string, string>> parameters = new()
        {
            new("origin", Origin),
            new("destination", Destination),
            new("departing", DepartureDate.ToString("yyyy-MM-dd"))
        };

        if (ReturnDate.HasValue)
        {
            // The search app models each leg as a repeated origin/destination/departing tuple.
            parameters.Add(new("origin", Destination));
            parameters.Add(new("destination", Origin));
            parameters.Add(new("departing", ReturnDate.Value.ToString("yyyy-MM-dd")));
        }

        parameters.Add(new("passengers", $"a{Adults}t{Teens}c{Children}i{Infants}"));

        if (AwardSearch)
        {
            parameters.Add(new("awardSearch", "true"));
        }

        if (!Cta.IsNullOrWhiteSpace())
        {
            parameters.Add(new("CTA", Cta));
        }

        string query = string.Join(
            "&",
            parameters.Select(parameter =>
                $"{Uri.EscapeDataString(parameter.Key)}={Uri.EscapeDataString(parameter.Value)}"));

        return $"/flights/search/slice?{query}";
    }
}

