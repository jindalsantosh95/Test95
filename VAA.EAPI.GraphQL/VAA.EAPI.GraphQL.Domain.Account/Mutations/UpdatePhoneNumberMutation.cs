using HotChocolate;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Account.Mappers;
using VAA.EAPI.GraphQL.Domain.Account.Models.ManageAccount;
using VAA.EAPI.GraphQL.Infrastructure.Entities;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using Envirnoment = VAA.EAPI.GraphQL.Common.Constants.Environment;

namespace VAA.EAPI.GraphQL.Domain.Account.Mutations
{
    [ExtendObjectType("Mutation")]
    public class UpdatePhoneNumberMutation
    {
        private readonly IOAuthTokenProvider _tokenProvider;
        private readonly IHttpConnector _connector;
        private readonly ILogger<UpdatePhoneNumberMutation> _logger;
        private readonly ITokenB2CProvider<string> _tokenB2CProvider;
        private readonly static Dictionary<string, string> msAuthToken = new();

        private readonly static Dictionary<string, string> headersCustomerMS = new Dictionary<string, string>()
        {
            {Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Envirnoment.CustomerOcpKey)}
        };

        public UpdatePhoneNumberMutation(ILogger<UpdatePhoneNumberMutation> logger, IOAuthTokenProvider tokenProvider, IHttpConnector connector, ITokenB2CProvider<string> tokenB2CProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
            _tokenB2CProvider = tokenB2CProvider ?? throw new ArgumentNullException(nameof(tokenB2CProvider));
        }

        [GraphQLName("updateMemberPhoneNumber")]
        [GraphQLDescription("Update Phone Number in Manage Account.")]
        public async Task<string> PhoneNumber(UpdatePhoneNumber updatePhoneNumber)
        {
            var jwtPrincipalFeature = AsyncContext.Current.Context.GetJwtPrincipalFeature();

            if (jwtPrincipalFeature?.AccessToken.Token.IsNullOrWhiteSpace() ?? true)
                throw new ArgumentNullException(nameof(System.Net.HttpRequestHeader.Authorization));

            try
            {
                var tokenResponse = await _tokenB2CProvider.ValidateTokenAsync(jwtPrincipalFeature.AccessToken.Token);
                var claim = Json.Deserialize<TokenB2CResponse>(tokenResponse);

                var customerResponse = _connector.GetAsync<CustomerDocument>(ConfigReader.Get(Common.Constants.Environment.CustomerMSURLForMemberDetails).Replace("{memberNumber}", claim?.FcNumber), headersCustomerMS, GetToken(ConfigReader.Get(Common.Constants.Environment.CustomerAppId)).GetAwaiter().GetResult()).GetAwaiter().GetResult();
                var updatePhoneNumberResponse = await _connector.SendAsync<ManageAccountUpdateResponse>(ConfigReader.Get(Envirnoment.UpdatePhoneNumber), headersCustomerMS, await GetToken(ConfigReader.Get(Common.Constants.Environment.CustomerAppId)), HttpMethod.Patch, Json.Serialize(UpdateMemberPhoneNumberMapper.GetUpdateMemberPhoneNumber(updatePhoneNumber, claim.FcNumber, customerResponse.Response.CustomerId)));

                return updatePhoneNumberResponse.Response.Value;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Update Phone number mutation failed");

                var apiResponse = await ex.ReadResponseContentAsStringAsync();
                var errorDetails = Json.Deserialize<List<ErrorDetails>>(apiResponse);

                if (!errorDetails.IsNullOrEmpty() && errorDetails.Any())
                {
                    var builder = ErrorBuilder.New().SetMessage("Phone number Update failed");
                    errorDetails.ForEach(error =>
                    {
                        builder.SetExtension("code", error.Code);
                        builder.SetExtension("message", error.Message);
                        builder.SetExtension("source", error.ErrorSource);
                    });

                    throw new GraphQLException(builder.Build());
                }

                throw new GraphQLException(ErrorBuilder.New()
                            .SetMessage($"Error while updating Phone number for Member \r\n{ex.Message}")
                            .SetExtension("Exception", ex.Message)
                            .SetCode(Constants.Application.MemberUpdatePhoneNumberError)
                            .Build());
            }

            catch (Exception ex)
            {
                _logger.LogCritical(ex, $"Unhandled Exception in Update Phone Number mutation \r\n{ex.Message}");
                throw new GraphQLException(ErrorBuilder.New()
                            .SetMessage($"Error while updating Phone Number for Member")
                            .SetExtension("Exception", ex.Message)
                            .SetCode(Constants.Application.MemberUpdatePhoneNumberError)
                            .Build());
            }

        }

        private async Task<string> GetToken(string appId)
        {
            return await _tokenProvider.AcquireTokenAsync(appId);
        }
    }
}
