namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Services.Entities;

public class StoreEntity
{
    public string StoreId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string EmailAddress { get; set; } = string.Empty;
    public string AddressLine1 { get; set; } = string.Empty;
    public string AddressLine2 { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string County { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string StoreImageSrc { get; set; } = string.Empty;
    public string StoreImageAlt { get; set; } = string.Empty;
    public string StoreWorkHoursJson { get; set; } = string.Empty;
    public string StoreSpecialHoursJson { get; set; } = string.Empty;
    public string? StoreServicesJson { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public bool MarkedAsClosed { get; set; }
}
