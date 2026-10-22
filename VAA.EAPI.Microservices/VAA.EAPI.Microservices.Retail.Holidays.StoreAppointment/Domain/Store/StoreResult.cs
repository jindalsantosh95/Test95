namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Domain.Store;

public class StoreResult
{
    public string StoreId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public StoreImage FeaturedImage { get; set; } = new();
    public IEnumerable<StoreImage> CardImages { get; set; } = Enumerable.Empty<StoreImage>();
    public string DrivingTime { get; set; } = string.Empty;
    public string Distance { get; set; } = string.Empty;
    public bool MarkedAsClosed { get; set; }
    public bool IsNew { get; set; } = false;
    public bool IsClosest { get; set; } = false;
    public StoreInformation? FullDetails { get; set; }
    public IEnumerable<StoreWorkHours> StoreWorkHours { get; set; } = Enumerable.Empty<StoreWorkHours>();
}
