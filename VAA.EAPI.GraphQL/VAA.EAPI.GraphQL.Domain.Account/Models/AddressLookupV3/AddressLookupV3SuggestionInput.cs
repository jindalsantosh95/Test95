namespace VAA.EAPI.GraphQL.Domain.Account.Models.AddressLookupV3
{
    public sealed class AddressLookupV3SuggestionInput
    {
        public string Query { get; set; }
        public string CountryCode { get; set; }
        public string PostCode { get; set; }
        public string AddressLine1 { get; set; }
        public string Reference { get; set; }
    }
}
