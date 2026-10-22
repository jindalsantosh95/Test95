using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Book.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Entities.VRedMS;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Book.Services
{
    public class VRedServiceProvider : IVRedServiceProvider
    {
        private static readonly Dictionary<string, string> _headers = new() { { Common.Constants.Application.OcpKeyHeader, ConfigReader.Try<string>(Common.Constants.Environment.VRedOcpSubID) } };

        private readonly IOAuthTokenProvider _tokenProvider;
        private readonly IHttpConnector _connector;

        public VRedServiceProvider(IHttpConnector connector, IOAuthTokenProvider tokenProvider)
        {
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
        }


        public async Task<VRedResponse> GetVirginPoints(string fcNumber)
        {
            // Get a token
            var token = await _tokenProvider.AcquireTokenAsync(ConfigReader.Try<string>(Common.Constants.Environment.BookVRedAppId));

            string getVRedResponse = ConfigReader.Try<string>(Common.Constants.Environment.BookVRedVirginPoints).Replace("{memberID}", fcNumber);

            return (await _connector.SendAsync<VRedResponse>(getVRedResponse, _headers, token, HttpMethod.Get, string.Empty)).Response;
        }


    }
}