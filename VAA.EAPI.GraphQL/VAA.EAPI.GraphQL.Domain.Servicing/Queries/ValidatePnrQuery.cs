using HotChocolate;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common.Constants;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Servicing.Models.ValidatePnr;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using Environment = VAA.EAPI.GraphQL.Common.Constants.Environment;

namespace VAA.EAPI.GraphQL.Domain.Servicing.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]

    public class ValidatePnrQuery
    {
        private readonly ILogger<ValidatePnrQuery> _logger;
        protected readonly IOAuthTokenProvider _tokenProvider;
        protected readonly IHttpConnector _connector;

        public ValidatePnrQuery(IOAuthTokenProvider tokenProvider, IHttpConnector connector, ILogger<ValidatePnrQuery> logger)
        {
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [GraphQLName("servicingValidatePnr")]
        [GraphQLDescription("Generates an authorization token for a PNR.")]

        public async Task<string?> AcquireToken(IdentityAcquireTokenRequest request)
        {
            try
            {
                _logger.LogDebug("Identity Call - Start");                
                var response = await _connector.SendAsync<string>($"{ConfigReader.Get(Environment.IdentityMsUrl)}{Application.TokenEndpoint}", GetHeaders(request), await GetTokenForIdentityMS(), HttpMethod.Post, string.Empty);
                _logger.LogDebug("Identity Call Successful - End");

                return response.Headers.GetValues("Authorization").FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred in ValidatePnrQuery - {ex.Message}", ex);
                throw;
            }
        }
        protected Dictionary<string, string?> GetHeaders(IdentityAcquireTokenRequest request)
        {
            return new Dictionary<string, string?>()
            {
                { "Type", request.Type},
                { "FirstName", request.FirstName},
                { "LastName", request.LastName},
                { "Pnr",request.RLOC },
                { "TicketRef",request.TicketRef },
                { Common.Constants.Application.OcpKeyHeader , ConfigReader.Get(Common.Constants.Environment.IdentityOcpKey)}
            };
        }
        protected async Task<string> GetTokenForIdentityMS() => await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.IdentityMSAppId));

    }
}
