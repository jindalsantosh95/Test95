using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Account.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Account.Infrastructure.Services
{
    public class RefDataService<T> : IRefDataService<T>
    {
        private readonly IHttpConnector _connector;
        private static readonly Dictionary<string, string> headers = new() { { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.RefDataMSOcpKey) } };

        public RefDataService(IHttpConnector connector)
        {
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        }

        public async Task<IEnumerable<T>> Query(params string[] filterValues)
        {
            filterValues = (filterValues ?? Array.Empty<string>()).OrderBy(x => x.ToLower()).ToArray();

            // Get the Url Suffix of the type we are requesting
            var attr = (RefDataTypeAttribute)typeof(T).GetCustomAttributes(typeof(RefDataTypeAttribute), true).First(); // Should always be one

            // Construct Url for the request
            string uri = string.Format(ConfigReader.Get(Common.Constants.Environment.RefDataMSGenericUrl), attr.UrlSuffix);

            if (!attr.FilterField.IsNullOrWhiteSpace())
                uri = $"{uri}?{attr.FilterField}={string.Join(',', filterValues)}";

            return (await _connector.GetAsync<IEnumerable<T>>(uri, headers, string.Empty)).Response;
        }
    }
}
