namespace VAA.EAPI.Microservices.Loyalty.AddressLookup.Domain.V3
{
    public sealed class AddressLookupV3SuggestionRequest
    {
        public string Query { get; set; }
        public string CountryCode { get; set; }
        public string Reference { get; set; }
    }
}
