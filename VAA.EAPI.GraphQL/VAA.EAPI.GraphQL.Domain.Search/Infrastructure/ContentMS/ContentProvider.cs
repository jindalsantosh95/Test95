using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Search.Interfaces;
using VAA.EAPI.GraphQL.Domain.Search.Models.SalesBanner;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Search.Infrastructure.ReferenceMS;

public sealed class ContentProvider : IContentProvider
{
    private readonly IHttpConnector _connector;

    public ContentProvider(IHttpConnector connector)
    {
        _connector = connector ?? throw new ArgumentNullException(nameof(connector));
    }

    public async Task<Dictionary<string, IEnumerable<SalesBanner>>> RetrieveSalesBannersAsync(SalesBannerFilters filters)
    {
        string salesBannersUri = $"{ConfigReader.Get(Common.Constants.Environment.ContentServiceUrl).TrimEnd('/')}/SalesBanners";

        var headers = new Dictionary<string, string>
        {
            { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.ContentServiceOcp) }
        };

        return (await _connector.SendAsync<Dictionary<string, IEnumerable<SalesBanner>>>(salesBannersUri, headers, string.Empty, HttpMethod.Post, Json.Serialize(filters))).Response;
    }


}
