namespace VAA.EAPI.GraphQL.Domain.Holidays.Infrastructure.Models.StoreLocator;

public class StoreRecord
{
    public string StoreId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string AddressLine1 { get; set; } = string.Empty;
    public string AddressLine2 { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string County { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public IEnumerable<StoreWorkHours> StoreWorkHours { get; set; } = Enumerable.Empty<StoreWorkHours>();
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public StoreImage FeaturedImage { get; set; } = new();
    public IEnumerable<StoreImage> CardImages { get; set; } = Enumerable.Empty<StoreImage>();
    public string Distance { get; set; } = string.Empty;
    public string DrivingTime { get; set; } = string.Empty;
    public bool MarkedAsClosed { get; set; }
    public bool IsNew { get; set; }
    public bool IsClosest { get; set; }
    public StoreInformation? FullDetails { get; set; }
}
