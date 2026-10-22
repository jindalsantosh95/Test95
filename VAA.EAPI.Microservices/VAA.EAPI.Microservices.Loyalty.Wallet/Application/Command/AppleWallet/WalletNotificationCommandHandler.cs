using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Azure.Cosmos.Serialization.HybridRow;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Microservices.Loyalty.Wallet.Domain;

namespace VAA.EAPI.Microservices.Loyalty.Wallet.Application.Command.AppleWallet
{
    public class WalletNotificationCommandHandler : IRequestHandler<WalletNotificationCommand, Result<Unit>>
    {
        private readonly IMediator _mediator;
        private readonly ITableStorageService<PushEntity> _tableStorageService;
        private readonly ILogger<RegisterPassCommandHandler> _logger;
        private readonly IHttpConnector _httpConnector;
        private readonly IOAuthTokenProvider _tokenProvider;

        public WalletNotificationCommandHandler(IMediator mediator, ITableStorageService<PushEntity> tableStorageService, ILogger<RegisterPassCommandHandler> logger, IHttpConnector httpConnector, IOAuthTokenProvider tokenProvider)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _tableStorageService = tableStorageService ?? throw new ArgumentNullException(nameof(tableStorageService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
        }

        public async Task<Result<Unit>> Handle(WalletNotificationCommand request, CancellationToken cancellationToken)
        {
            bool response = true;
            try
            {
                List<PushEntity> pushData = await _tableStorageService.RetrieveItemsAsync(default);

                if (pushData != null)
                {
                    int recordCount = Math.Min(Convert.ToInt32(Json.Deserialize<string>(Convert.ToString(request.Data))), pushData.Count);

                    for (var i = 0; i < recordCount; i++)
                    {
                        var pushEntity = pushData[i];
                        var customerResponse = await _httpConnector.GetAsync<CustomerResponse>($"{ConfigReader.Get(Constants.Environment.CustomerMsUrl)}{Constants.Application.GetCustomerDtlsByMemberID}".Replace("{memberid}", pushEntity.SerialNumber), GetHeaders(), await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Constants.Environment.CustomerAppId)));

                        if (customerResponse.Customer.MemberTier != "Red")
                            response = (await _mediator.Send(new SendPushNotificationCommand() { CustomerId = customerResponse.Customer.CustomerId, TierCode = customerResponse.Customer.MemberTier })).Value;

                        if (response)
                            await _tableStorageService.DeleteAsync(pushEntity);
                    }
                }

                var result = new Response<string>
                {
                    Status = Status.OK,
                    Value = "Wallet Notification Batch has ran successfully."
                };
                await _mediator.Send(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = Json.Serialize(result), Status = ServiceResponseProcessStatus.Completed, Type = Constants.Application.WalletNotification }, cancellationToken);

                return Unit.Value;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while pushing notification with error {ex.Message}");

                var result = new Response<string>
                {
                    Status = Status.BusinessException,
                    Value = "Wallet Notification Batch Failed to Run. Please rerun the Batch"
                };
                await _mediator.Send(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = Json.Serialize(result), Status = ServiceResponseProcessStatus.Error, Type = Constants.Application.WalletNotification }, cancellationToken);

                return Unit.Value;
            }

        }
        private static Dictionary<string, string> GetHeaders()
        {
            return new Dictionary<string, string>
            {
                {   Constants.Application.OcpKeyHeader , ConfigReader.Get(Constants.Environment.EapiOcpSubId) }
            };
        }
    }
}
