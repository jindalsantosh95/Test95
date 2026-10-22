using InfrastructureStoreManagerStoreResponse = VAA.EAPI.GraphQL.Domain.Holidays.Infrastructure.Models.StoreManager.StoreResponse;

namespace VAA.EAPI.GraphQL.Domain.Holidays.Models.StoreManager;

public class StoreManagerStoreResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string AddressLine1 { get; set; } = string.Empty;
    public string AddressLine2 { get; set; } = string.Empty;
    public string CityTown { get; set; } = string.Empty;
    public string CountyRegion { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Telephone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public IEnumerable<string> Services { get; set; } = Enumerable.Empty<string>();
    public bool IsClosed { get; set; }

    public static explicit operator StoreManagerStoreResponse(InfrastructureStoreManagerStoreResponse response)
    {
        ArgumentNullException.ThrowIfNull(response);

        return new StoreManagerStoreResponse
        {
            Id = response.Id,
            Name = response.Name,
            Label = response.Label,
            AddressLine1 = response.AddressLine1,
            AddressLine2 = response.AddressLine2,
            CityTown = response.CityTown,
            CountyRegion = response.CountyRegion,
            PostalCode = response.PostalCode,
            Telephone = response.Phone,
            Email = response.Email,
            Services = response.Services ?? Enumerable.Empty<string>(),
            IsClosed = response.IsClosed
        };
    }
}
