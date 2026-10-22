using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Book.Interfaces;
using VAA.EAPI.GraphQL.Domain.Book.Models.Request.Loyalty;
using VAA.EAPI.GraphQL.Domain.Book.Models.Response.Loyalty;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Book.Services
{
    public class LoyaltySyncServiceProvider : ILoyaltySyncServiceProvider
    {
        private static readonly Dictionary<string, string> _headers = new() { { Common.Constants.Application.OcpKeyHeader, ConfigReader.Try<string>(Common.Constants.Environment.LoyaltySyncOcpSubId) } };

        private readonly IOAuthTokenProvider _tokenProvider;
        private readonly IHttpConnector _connector;

        public LoyaltySyncServiceProvider(IHttpConnector connector, IOAuthTokenProvider tokenProvider)
        {
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
        }

        public async Task<IEnumerable<PointsCurrencyConverterResponse>> PointsCurrencyConverter(IEnumerable<PointsCurrencyConverterRequest> pointsCurrencyConverterMSRequest)
        {
            string uri = $"{ConfigReader.Get(Common.Constants.Environment.LoyaltySync).TrimEnd('/')}/PointsCurrencyConverter";
            return (await _connector.SendAsync<IEnumerable<PointsCurrencyConverterResponse>>(uri, _headers, string.Empty, HttpMethod.Post, Json.Serialize(pointsCurrencyConverterMSRequest))).Response;
        }
    }
}