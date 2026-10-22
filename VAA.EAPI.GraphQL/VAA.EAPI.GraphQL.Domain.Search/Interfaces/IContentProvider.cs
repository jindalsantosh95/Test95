using VAA.EAPI.GraphQL.Domain.Search.Models.SalesBanner;

namespace VAA.EAPI.GraphQL.Domain.Search.Interfaces
{
    public interface IContentProvider
    {
        Task<Dictionary<string, IEnumerable<SalesBanner>>> RetrieveSalesBannersAsync(SalesBannerFilters filters);
    }
}
