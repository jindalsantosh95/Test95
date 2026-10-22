namespace VAA.EAPI.GraphQL.Domain.Holidays.Models.StoreLocator
{
    public class StoreContactDetails
    {
        public string Phone { get; set; } = default!;
        public string EmailAddress { get; set; } = string.Empty;
        public StoreAddress Address { get; set; } = new();
    }
}
