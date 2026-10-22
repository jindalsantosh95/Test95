namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Services.StoreManager.Models;

internal sealed class StoreManagerStoreRow
{
    public string? StoreId { get; set; }
    public string? Name { get; set; }
    public string? Label { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? County { get; set; }
    public string? PostalCode { get; set; }
    public string? CountryCode { get; set; }
    public string? Phone { get; set; }
    public string? EmailAddress { get; set; }
    public string? StoreServicesJson { get; set; }
    public bool MarkedAsClosed { get; set; }
}
