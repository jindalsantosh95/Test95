namespace VAA.EAPI.GraphQL.Domain.Holidays.Infrastructure.Models.StoreLocator;

public class StoreContactDetails
{
    public string Phone { get; set; } = string.Empty;
    public string EmailAddress { get; set; } = string.Empty;
    public StoreAddress Address { get; set; } = new();
}
