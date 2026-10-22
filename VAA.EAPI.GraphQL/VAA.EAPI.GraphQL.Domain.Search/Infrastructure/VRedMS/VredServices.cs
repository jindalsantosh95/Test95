using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Search.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Entities.VRedMS;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Search.Infrastructure.VRedMS
{
    public class VredServices : IVRedServices
    {
        private readonly IOAuthTokenProvider _tokenProvider;
        private readonly IHttpConnector _connector;
        private readonly ILogger<VredServices> _logger;


        public VredServices(IOAuthTokenProvider tokenProvider, IHttpConnector connector, ILogger<VredServices> logger)
        {
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<VRedResponse> GetVRedPoints(string flyingClubNumber)
        {
            try
            {
                string vredPointUrl = $"{ConfigReader.Get(Common.Constants.Environment.BookVRedVirginPoints)}".Replace("{memberID}", flyingClubNumber);

                var vredtoken = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.BookVRedAppId));

                Dictionary<string, string> headers = new() { { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.VRedOcpSubID) } };

                return _connector.GetAsync<VRedResponse>(vredPointUrl, headers, vredtoken).Result.Response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"FCNumber - {flyingClubNumber} Correlation Id - {AsyncContext.Current.CorrelationId} - Exception - {ex.Message}", ex);
                return null;
            }
        }
    }
}
