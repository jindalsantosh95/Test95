namespace VAA.EAPI.GraphQL.Domain.Account.Models
{
    public class AddressDetailRequest
    {
        public string CountryCode { get; set; }
        public string PostCode { get; set; }
        public string AddressLine1 { get; set; }
    }
}
