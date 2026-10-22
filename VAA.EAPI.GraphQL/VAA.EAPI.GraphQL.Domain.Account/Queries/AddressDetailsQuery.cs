using HotChocolate;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Account.Models;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Account.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class AddressDetailsQuery
    {
        private readonly IOAuthTokenProvider _oauthTokenProvider;
        private readonly IHttpConnector _httpConnector;
        private readonly ILogger<AddressDetailsQuery> _logger;
        public AddressDetailsQuery(IOAuthTokenProvider oauthTokenProvider, IHttpConnector httpConnector, ILogger<AddressDetailsQuery> logger)
        {
            _oauthTokenProvider = oauthTokenProvider ?? throw new ArgumentNullException(nameof(oauthTokenProvider));
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [GraphQLName("accountAddressDetails")]
        [GraphQLDescription("Retrieve Address Details for US and UK postcode/zipcode")]
        public async Task<List<WesalAddressResponse>> FCAddressDetails(AddressDetailRequest request)
        {
            try
            {
                _logger.LogInformation("Retrieving Address Details");
                if (request == null)
                    throw new ArgumentNullException(nameof(request));

                if (ConfigReader.Get<bool>(Common.Constants.Environment.EnableAzureEndpointAddressDetails))
                {
                    var apiResponse = await _httpConnector.SendAsync<List<WesalAddressResponse>>(ConfigReader.Get(Common.Constants.Environment.AddressLookUpURLForAddressDetailsV2), GetHeaders(), await _oauthTokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.AddressLookupAPIAppId)), HttpMethod.Post, Json.Serialize(request));
                    return apiResponse.Response;
                }
                else
                {
                    var apiResponse = await _httpConnector.SendAsync<List<WesalAddressResponse>>(ConfigReader.Get(Common.Constants.Environment.AddressLookUpURLForAddressDetails), GetHeaders(), await _oauthTokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.AddressLookupAPIAppId)), HttpMethod.Post, Json.Serialize(request));
                    return apiResponse.Response;
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in AddressDetails Query");
                throw;
            }
        }
        private static Dictionary<string, string> GetHeaders()
        {
            return new Dictionary<string, string>()
            {
                {  Common.Constants.Application.OcpKeyHeader , ConfigReader.Get(Common.Constants.Environment.EnrolmentMSOcpKey) }
            };
        }
    }
}
