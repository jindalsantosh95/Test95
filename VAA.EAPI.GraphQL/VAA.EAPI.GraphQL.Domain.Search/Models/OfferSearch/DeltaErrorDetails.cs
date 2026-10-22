using VAA.EAPI.GraphQL.Common.Entities;

namespace VAA.EAPI.GraphQL.Domain.Search.Models.OfferSearch
{
    public class DeltaErrorDetails : ErrorDetails
    {
        public string? ThirdPartyServiceErrorCode { get; set; }
    }
}
