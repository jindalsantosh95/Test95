using CSharpFunctionalExtensions;
using MediatR;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Wallet.Domain;
using VAA.EAPI.Microservices.Loyalty.Wallet.Constants;
using System.Linq;
using VAA.EAPI.Microservices.Loyalty.Wallet.Helper;
using Microsoft.Extensions.Logging;
using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Microservices.Loyalty.Wallet.Application.Command.AppleWallet
{
    public class SendPushNotificationCommandHandler : IRequestHandler<SendPushNotificationCommand, Result<bool>>
    {
        private readonly IOAuthTokenProvider _tokenProvider;
        private readonly IHttpConnector _httpConnector;
        private readonly ITableStorageService<PassEntities> _tableStorageService;
        private readonly ILogger<SendPushNotificationCommandHandler> _logger;


        public SendPushNotificationCommandHandler(IOAuthTokenProvider tokenProvider, ILogger<SendPushNotificationCommandHandler> logger, IHttpConnector httpConnector, ITableStorageService<PassEntities> tableStorageService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
            _tableStorageService = tableStorageService ?? throw new ArgumentNullException(nameof(tableStorageService));
        }

        public async Task<Result<bool>> Handle(SendPushNotificationCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var customerResponse = await _httpConnector.SendAsync<CustomerResponse>($"{ConfigReader.Get(Constants.Environment.CustomerMsUrl)}{Constants.Application.GetCustomerDetails}".Replace("{customerId}", request.CustomerId), GetHeaders(), await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Constants.Environment.CustomerAppId)), HttpMethod.Get, string.Empty);

                if (customerResponse.Customer.MemberTier == request.TierCode)
                {
                    var passRegisteredData = await _tableStorageService.RetrieveItemsAsync(Constants.Application.WalletPassEvent, Constants.Application.SerialNumber, customerResponse.Customer.LoyaltyNumber, default);
                    if (passRegisteredData != null && passRegisteredData.Any())
                    {
                        foreach (var passData in passRegisteredData)
                        {
                            await PushNotificationService.PushNotification(_httpConnector, passData.PushToken);
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while sending push notification. {ex.Message}");
                return false;
            }
        }

        private static Dictionary<string, string> GetHeaders()
        {
            return new Dictionary<string, string>
            {
                {  Constants.Application.OcpKeyHeader , ConfigReader.Get(Constants.Environment.EapiOcpSubId) }
            };
        }
    }
}
