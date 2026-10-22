using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Book.Interfaces;
using VAA.EAPI.GraphQL.Domain.Book.Models.Insurance.InsuranceQuoteMS;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Book.Services
{
    public class InsuranceServiceProvider : IInsuranceServiceProvider
    {
        private static readonly Dictionary<string, string> _headers = new() { { Common.Constants.Application.OcpKeyHeader, ConfigReader.Try<string>(Constants.Environment.InsuranceMSOcpKey) } };

        private readonly IOAuthTokenProvider _tokenProvider;
        private readonly IHttpConnector _connector;

        public InsuranceServiceProvider(IHttpConnector connector)
        {
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        }


        public async Task<InsuranceQuoteResponse> InsuranceQuote(InsuranceQuoteRequest request)
        {
            string getQuoteUrl = ConfigReader.Try<string>(Constants.Environment.InsuranceMSUrl);

            return (await _connector.SendAsync<InsuranceQuoteResponse>(getQuoteUrl, _headers, null, HttpMethod.Post, Json.Serialize(request))).Response;
        }


    }
}