using System.Collections.Generic;

namespace VAA.EAPI.GraphQL.Domain.Account.Models.AddressLookupV3
{
    public sealed class AddressLookupV3SuggestionResponse
    {
        public string FlowId { get; set; }
        public IEnumerable<AddressLookupV3Candidate> Candidates { get; set; }
    }
}
