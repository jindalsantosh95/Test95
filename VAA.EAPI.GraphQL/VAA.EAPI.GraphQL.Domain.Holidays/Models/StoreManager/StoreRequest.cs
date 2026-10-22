using Newtonsoft.Json;

namespace VAA.EAPI.GraphQL.Domain.Holidays.Models.StoreManager;

public class StoreRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Label { get; set; }
    public string AddressLine1 { get; set; } = string.Empty;
    public string? AddressLine2 { get; set; }
    public string CityTown { get; set; } = string.Empty;
    public string? CountyRegion { get; set; }
    public string PostalCode { get; set; } = string.Empty;
    public string? Telephone { get; set; }
    public string? Email { get; set; }
    public IEnumerable<string> Services { get; set; } = Enumerable.Empty<string>();
    public bool IsClosed { get; set; }
}
