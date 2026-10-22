using Newtonsoft.Json;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.Request.VoucherRedemption
{
    public sealed class VoucherPnrUpdateRequest
    {
        public string CustomerId { get; init; }
        public string MemberNumber { get; init; }
        public string InitiatedBy { get; init; }
        public IEnumerable<string> VoucherNumbers { get; init; }

        [JsonProperty("pNR")]
        public string Pnr { get; init; }
    }
}
