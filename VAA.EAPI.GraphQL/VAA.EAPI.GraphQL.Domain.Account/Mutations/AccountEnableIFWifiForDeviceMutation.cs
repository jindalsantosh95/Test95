using HotChocolate;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Models.ManageAccount;
using VAA.EAPI.GraphQL.Infrastructure.Entities;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services.Analytics;
using ApplicationConstants = VAA.EAPI.GraphQL.Common.Constants.Application;
using Environment = VAA.EAPI.GraphQL.Common.Constants.Environment;


namespace VAA.EAPI.GraphQL.Domain.Account.Mutations
{
    [ExtendObjectType("Mutation")]
    public class AccountEnableIFWifiForDeviceMutation
    {
        private readonly IInflightConnectivityMutationProvider _inflightConnectivityMutationProvider;
        private readonly ITokenB2CProvider<string> _tokenB2CProvider;
        private readonly ILogger<AccountEnableIFWifiForDeviceMutation> _logger;
        private readonly IAnalyticsService _analyticsService;
        private readonly int _limitedExpiry = ConfigReader.Get<int>(Environment.MyAccountInflightConnectivityLimitedExpiry);
        private readonly int _expiry = ConfigReader.Get<int>(Environment.MyAccountInflightConnectivityExpiry);



        public AccountEnableIFWifiForDeviceMutation(
            IInflightConnectivityMutationProvider inflightConnectivityMutationProvider,
            ITokenB2CProvider<string> tokenB2CProvider,
            ILogger<AccountEnableIFWifiForDeviceMutation> logger,
            IAnalyticsService analyticsService)
        {
            _inflightConnectivityMutationProvider = inflightConnectivityMutationProvider ?? throw new ArgumentNullException(nameof(inflightConnectivityMutationProvider));
            _tokenB2CProvider = tokenB2CProvider ?? throw new ArgumentNullException(nameof(tokenB2CProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _analyticsService = analyticsService ?? throw new ArgumentNullException(nameof(analyticsService));

        }

        [GraphQLName("accountEnableIFWifiForDevice")]
        [GraphQLDescription("Enable IF Wifi for a device in sandbox.")]
        public async Task<AccountInflightDeviceStateResponse> EnableIFWifiForDevice(string deviceId, string sandboxId)
        {
            if (string.IsNullOrWhiteSpace(deviceId))
            {
                throw new ArgumentNullException(nameof(deviceId));
            }

            if (string.IsNullOrWhiteSpace(sandboxId))
            {
                throw new ArgumentNullException(nameof(sandboxId));
            }

            var request = new AccountEnableIFWifiForDeviceRequest
            {
                DeviceId = deviceId,
                SandboxId = sandboxId
            };

            var jwtPrincipalFeature = AsyncContext.Current.Context.GetJwtPrincipalFeature();
            if (!(jwtPrincipalFeature?.AccessToken.Token.IsNullOrWhiteSpace() ?? true))
            {
                try
                {
                    var tokenResponse = await _tokenB2CProvider.ValidateTokenAsync(jwtPrincipalFeature.AccessToken.Token);
                    if (!string.IsNullOrWhiteSpace(Json.Deserialize<TokenB2CResponse>(tokenResponse)?.FcNumber))
                    {
                        request.Expiry = DateTime.UtcNow.AddDays(_expiry);
                    }
                    else
                    {
                        request.Expiry = DateTime.UtcNow.AddMinutes(_limitedExpiry);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unhandled Exception in AccountEnableIFWifiForDevice mutation");
                    request.Expiry = DateTime.UtcNow.AddMinutes(_limitedExpiry);
                }
            }
            else
            {
                request.Expiry = DateTime.UtcNow.AddMinutes(_limitedExpiry);
            }

            return await _inflightConnectivityMutationProvider.CreateInflightConnectivity(request);
        }

        [GraphQLName("setDeviceInflightState")]
        [GraphQLDescription("Apply inflight state to device")]
        public async Task<AccountInflightDeviceStateResponse> SetDeviceInflightState(string deviceId, string sandboxId, [GraphQLName("EnableType")] DeviceInflightEnableType? enableType = null)
        {
            if (string.IsNullOrWhiteSpace(deviceId))
            {
                throw new ArgumentNullException(nameof(deviceId));
            }

            if (string.IsNullOrWhiteSpace(sandboxId))
            {
                throw new ArgumentNullException(nameof(sandboxId));
            }

            var request = new AccountEnableIFWifiForDeviceRequest
            {
                DeviceId = deviceId,
                SandboxId = sandboxId,
                Expiry = DateTime.UtcNow.AddDays(_expiry)
            };

            if (enableType.HasValue)
            {
                _ = TrackSetDeviceInflightStateAnalyticsAsync(enableType.Value);
            }

            return await _inflightConnectivityMutationProvider.CreateInflightConnectivity(request);
        }

        private async Task TrackSetDeviceInflightStateAnalyticsAsync(DeviceInflightEnableType enableType)
        {
            var analyticsData = new Dictionary<string, object>
            {
                { "EnableType", enableType.ToString() }
            };

            await _analyticsService.TrackAnalytics(
                (TrackAnalyticsMessage)(
                    Type: ApplicationConstants.TrackingAnalyticsType,
                    Subject: "SetDeviceInflightState",
                    Data: analyticsData));
        }
    }
}
