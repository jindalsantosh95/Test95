using VAA.EAPI.GraphQL.Common.Entities;

namespace VAA.EAPI.GraphQL.Domain.Search.Models.OfferSearch
{
    public class DeltaErrorResponse : ErrorDetails
    {
        public IEnumerable<MoreInfo> MoreInfo { get; set; }

    }
}
