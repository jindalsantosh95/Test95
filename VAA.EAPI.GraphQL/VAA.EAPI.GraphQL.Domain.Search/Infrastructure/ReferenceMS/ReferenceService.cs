using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Search.Interfaces;
using VAA.EAPI.GraphQL.Domain.Search.Models.SalesBanner;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Search.Infrastructure.ReferenceMS;

public sealed class ReferenceService : IReferenceService
{
    private readonly IHttpConnector _connector;

    public ReferenceService(IHttpConnector connector)
    {
        _connector = connector ?? throw new ArgumentNullException(nameof(connector));
    }

    public async Task<Dictionary<string, IEnumerable<SalesBanner>>> RetrieveSalesBannersAsync(SalesBannerFilters filters)
    {
        string salesBannersUri = string.Format(ConfigReader.Get(Common.Constants.Environment.RefDataMSGenericUrl), "salesBanners");

        return (await _connector.SendAsync<Dictionary<string, IEnumerable<SalesBanner>>>(salesBannersUri, GetHeaders(), string.Empty, HttpMethod.Post, Json.Serialize(filters))).Response;
    }

    private static Dictionary<string, string> GetHeaders()
    {
        return new Dictionary<string, string>()
        {
            { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Constants.Environment.ReferenceMSOcpKey) },
        };
    }
}
