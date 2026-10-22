using VAA.EAPI.GraphQL.Common.Entities;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models.BulkMemberUpdate
{
    public sealed class BulkMemberUpdateResponse
    {
        public string Message { get; set; }
        public IEnumerable<ErrorDetails>? Errors { get; set; }
    }
}
