using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Search.Interfaces;
using VAA.EAPI.GraphQL.Domain.Search.Models.Brand;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Search.Infrastructure.BrandMS
{
    public class BrandService : IBrandService
    {
        protected readonly IHttpConnector _connector;

        private static readonly Dictionary<string, string> headers = new() { { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Constants.Environment.BrandMsOcpKey) } };

        public BrandService(IHttpConnector connector)
        {
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        }

        public async Task<Brand> GetBrandAsync(BrandQueryRequest brandRQ)
        {
            var brandMSRequest = Json.Serialize(brandRQ);

            return (await _connector.SendAsync<Brand>(ConfigReader.Get(Constants.Environment.BrandMsUrl), headers, string.Empty, HttpMethod.Post, brandMSRequest)).Response;
        }
    }
}