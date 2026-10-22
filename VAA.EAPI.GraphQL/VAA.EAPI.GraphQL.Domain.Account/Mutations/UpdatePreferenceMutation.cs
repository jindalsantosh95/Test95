using HotChocolate;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Account.Mappers.ManageAccount;
using VAA.EAPI.GraphQL.Domain.Account.Models.ManageAccount;
using VAA.EAPI.GraphQL.Infrastructure.Entities;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using Environment = VAA.EAPI.GraphQL.Common.Constants.Environment;

namespace VAA.EAPI.GraphQL.Domain.Account.Mutations
{
    [ExtendObjectType("Mutation")]

    public class UpdatePreferenceMutation
    {
        private readonly ILogger<UpdatePreferenceMutation> _logger;
        private readonly IOAuthTokenProvider _tokenProvider;
        private readonly IHttpConnector _connector;
        private readonly ITokenB2CProvider<string> _tokenB2CProvider;

        // Use Lazy<Task<string>> to prevent concurrent token stampedes
        private static readonly ConcurrentDictionary<string, Lazy<Task<string>>> msAuthTokenLazy = new();

        public UpdatePreferenceMutation(ILogger<UpdatePreferenceMutation> logger, IOAuthTokenProvider tokenProvider, IHttpConnector connector, ITokenB2CProvider<string> tokenB2CProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
            _tokenB2CProvider = tokenB2CProvider ?? throw new ArgumentNullException(nameof(tokenB2CProvider));
        }

        [GraphQLName("updatePreference")]
        [GraphQLDescription("Update Preference in Manage Account.")]
        public async Task<bool> UpdatePreference(UpdatePreferenceRequest updatePreferenceRequest)
        {
            var jwtPrincipalFeature = AsyncContext.Current.Context.GetJwtPrincipalFeature();

            if (jwtPrincipalFeature?.AccessToken.Token.IsNullOrWhiteSpace() ?? true)
                throw new ArgumentNullException(nameof(System.Net.HttpRequestHeader.Authorization));

            try
            {
                if (updatePreferenceRequest?.Preferences == null || !updatePreferenceRequest.Preferences.Any())
                {
                    throw new GraphQLException(ErrorBuilder.New()
                            .SetMessage("Preferences are required")
                            .SetExtension("Validation", "Preferences are required")
                            .SetCode(Constants.Application.MemberUpdatePreferenceError)
                            .Build());
                }

                var tokenResponse = await _tokenB2CProvider.ValidateTokenAsync(jwtPrincipalFeature.AccessToken.Token);

                var claim = Json.Deserialize<TokenB2CResponse>(tokenResponse);

                if (claim == null || claim.FcNumber.IsNullOrWhiteSpace())
                {
                    throw new GraphQLException(ErrorBuilder.New()
                            .SetMessage("Member number is missing from token claims")
                            .SetExtension("Validation", "Member number is missing from token claims")
                            .SetCode(Constants.Application.MemberUpdatePreferenceError)
                            .Build());
                }

                var customerResponse = await _connector.GetAsync<CustomerDocument>(ConfigReader.Get(Environment.CustomerMSURLForMemberDetails).Replace("{memberNumber}", claim.FcNumber), new Dictionary<string, string>() { { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Environment.CustomerOcpKey) } }, await GetToken(ConfigReader.Get(Environment.CustomerAppId), Common.Constants.Application.CustomerMS));

                var response = await _connector.SendAsync<UpdatePreferenceResponse>($"{ConfigReader.Get(Environment.CustTransactionMsUrl).TrimEnd('/')}/UpdatePreferences", new Dictionary<string, string>() { { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Environment.CustTransOcpKey) } }, await GetToken(ConfigReader.Get(Environment.CustTransAppId), Common.Constants.Application.CustomerTransaction), HttpMethod.Patch, Json.Serialize(UpdatePreferenceRequestMapper.GetUpdatedPreference(claim, updatePreferenceRequest, customerResponse.Response)));

                if (response.Response.ErrorDetails != null && response.Response.ErrorDetails.Any())
                {
                    var builder = ErrorBuilder.New()
                        .SetMessage("Update Preference failed")
                        .SetCode(response.Response.ErrorDetails.First().Code)
                        .SetExtension("errors", response.Response.ErrorDetails.Select(e => new
                        {
                            code = e.Code,
                            message = e.Message,
                            source = e.ErrorSource
                        }).ToList());

                    throw new GraphQLException(builder.Build());
                }

                return response.Response.PreferenceValue;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Update Preference Mutation Failed");

                var apiResponse = await ex.ReadResponseContentAsStringAsync();
                List<ErrorDetails>? errorDetails = null;

                if (!string.IsNullOrWhiteSpace(apiResponse))
                {
                    errorDetails = Json.Deserialize<List<ErrorDetails>>(apiResponse);
                }

                if (errorDetails?.Count > 0)
                {
                    var builder = ErrorBuilder.New()
                        .SetMessage("Update Preference failed")
                        .SetCode(Constants.Application.MemberUpdatePreferenceError)
                        .SetExtension("errors", errorDetails.Select(e => new
                        {
                            code = e.Code,
                            message = e.Message,
                            source = e.ErrorSource
                        }).ToList());

                    throw new GraphQLException(builder.Build());
                }

                throw new GraphQLException(ErrorBuilder.New()
                            .SetMessage($"Error while updating Preference for Member \r\n{ex.Message}")
                            .SetExtension("Exception", ex.Message)
                            .SetCode(Constants.Application.MemberUpdatePreferenceError)
                            .Build());
            }
            catch (Exception ex) when (ex is not GraphQLException)
            {
                _logger.LogCritical(ex, $"Update Preference Mutation Failed \r\n{ex.Message}");
                throw new GraphQLException(ErrorBuilder.New()
                            .SetMessage($"Error while updating Preference for Member")
                            .SetExtension("Exception", ex.Message)
                            .SetCode(Constants.Application.MemberUpdatePreferenceError)
                            .Build());
            }
        }

        private async Task<string> GetToken(string appId, string microService)
        {
            var lazyToken = await AcquireToken(appId, microService);

            // Remove and retry if token is invalid
            if (!lazyToken.IsJwtTokenValid())
            {
                msAuthTokenLazy.TryRemove($"{microService}:{appId}", out _);
                return await AcquireToken(appId, microService);
            }

            return lazyToken;
        }

        private async Task<string> AcquireToken(string appId, string microService)
        {
            var lazyToken = msAuthTokenLazy.GetOrAdd($"{microService}:{appId}", _ => new Lazy<Task<string>>(async () =>
            {
                var token = await _tokenProvider.AcquireTokenAsync(appId);
                return token;
            }));

            try
            {
                return await lazyToken.Value;
            }
            catch (Exception ex)
            {
                msAuthTokenLazy.TryRemove($"{microService}:{appId}", out _);

                _logger.LogError(ex, "Error while acquiring token for microService {MicroService} and appId {AppId}", microService, appId);

                throw new GraphQLException(ErrorBuilder.New()
                            .SetMessage("Error while acquiring token.")
                            .SetCode(Constants.Application.MemberUpdatePreferenceError)
                            .Build());
            }
        }

    }
}

