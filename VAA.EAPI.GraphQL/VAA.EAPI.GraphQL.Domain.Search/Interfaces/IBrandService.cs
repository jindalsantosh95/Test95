using VAA.EAPI.GraphQL.Domain.Search.Models.Brand;

namespace VAA.EAPI.GraphQL.Domain.Search.Interfaces;

public interface IBrandService
{
    Task<Brand> GetBrandAsync(BrandQueryRequest brandRQ);
}