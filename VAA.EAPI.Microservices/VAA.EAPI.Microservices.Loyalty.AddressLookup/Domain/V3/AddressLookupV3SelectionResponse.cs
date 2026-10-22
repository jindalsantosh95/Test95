using VAA.EAPI.Microservices.Loyalty.AddressLookup.Domain.Entities;

namespace VAA.EAPI.Microservices.Loyalty.AddressLookup.Domain.V3
{
    public sealed class AddressLookupV3SelectionResponse
    {
        public string FlowId { get; set; }
        public AddressDetail Address { get; set; }
    }
}
