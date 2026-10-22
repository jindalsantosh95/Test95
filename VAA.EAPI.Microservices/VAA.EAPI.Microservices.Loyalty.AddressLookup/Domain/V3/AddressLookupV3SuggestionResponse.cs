using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Loyalty.AddressLookup.Domain.V3
{
    public sealed class AddressLookupV3SuggestionResponse
    {
        public string FlowId { get; set; }
        public IEnumerable<AddressLookupV3Candidate> Candidates { get; set; }
    }
}
