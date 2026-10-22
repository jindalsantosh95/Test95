using System;

namespace VAA.EAPI.Microservices.Loyalty.AddressLookup.Infrastructure.V3
{
    public sealed class AddressLookupV3Reference
    {
        public string ContractVersion { get; set; }
        public string ProviderId { get; set; }
        public string ItemType { get; set; }
        public string FlowId { get; set; }
        public string SelectionIntentId { get; set; }
        public string CountryCode { get; set; }
        public DateTimeOffset ExpiresAtUtc { get; set; }
    }
}
