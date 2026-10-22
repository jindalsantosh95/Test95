using VAA.EAPI.Microservices.Retail.Holidays.PriceFeed.Domain;

public static class PricingFeedCsvBuilder
{
    public const string Header =
        "destination_id,address,url,price,status,type,length_of_stay,num_travelers,travel_start,travel_end,departure_airport,name,latitude,longitude,description";

    public static string ToCsvLine(GenaratePricingFeedDataResponse row)
    {
        return string.Join(',', new[]
        {
            Escape(row.DestinationId),
            Escape(row.Address),
            Escape(row.Url),
            Escape(row.Price),
            Escape(row.Status),
            Escape(row.Type),
            Escape(row.LengthOfStay),
            Escape(row.NumTravelers),
            Escape(row.TravelStart),
            Escape(row.TravelEnd),
            Escape(row.DepartureAirport),
            Escape(row.Name),
            Escape(row.Latitude),
            Escape(row.Longitude),
            Escape(row.Description)
        });
    }

    private static string Escape(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var cleaned = value
            .Replace("\t", " ")
            .Replace("\r", " ")
            .Replace("\n", " ");

        if (cleaned.Contains(',') || cleaned.Contains('"'))
        {
            cleaned = cleaned.Replace("\"", "\"\"");
            return $"\"{cleaned}\"";
        }

        return cleaned;
    }
}
