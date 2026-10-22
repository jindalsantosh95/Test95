using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Microservices.Loyalty.Wallet.Domain;

namespace VAA.EAPI.Microservices.Loyalty.Wallet.Application.Command.AppleWallet
{
    public class RegisterPassCommandHandler : IRequestHandler<RegisterPassCommand, Response<int>>
    {
        private readonly ITableStorageService<PassEntities> _tableStorageService;
        private readonly ILogger<RegisterPassCommandHandler> _logger;

        public RegisterPassCommandHandler(ITableStorageService<PassEntities> tableStorageService, ILogger<RegisterPassCommandHandler> logger)
        {
            _tableStorageService = tableStorageService ?? throw new ArgumentNullException(nameof(tableStorageService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Response<int>> Handle(RegisterPassCommand request, CancellationToken cancellationToken)
        {
            string rowKey = $"{request.DeviceId}{Constants.Application.Separator}{request.PassSerial}";

            try
            {
                var passRegisteredData = await _tableStorageService.RetrieveAsync(Constants.Application.WalletPassEvent, rowKey);

                if (passRegisteredData is not null)
                {
                    _logger.LogInformation($"Serial number {request.PassSerial} already register for device id : {request.DeviceId} and push token {request.PushToken}");

                    return new Response<int>()
                    {
                        Status = Status.OK,
                        Value = 200,
                        Errors = null
                    };
                }
                else
                {
                    PassEntities passEntities = new()
                    {
                        PassTypeId = request.PassTypeId,
                        PartitionKey = Constants.Application.WalletPassEvent,
                        PushToken = request.PushToken,
                        RowKey = rowKey,
                        SerialNumber = request.PassSerial,
                        DeviceId = request.DeviceId,
                        LastUpdated = DateTime.UtcNow.ToString()
                    };

                    await _tableStorageService.InsertOrMergeAsync(passEntities);

                    return new Response<int>()
                    {
                        Status = Status.OK,
                        Value = 201,
                        Errors = null
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while registering passes for deviceID: {request.DeviceId} with error {ex.Message}");

                return new Response<int>()
                {
                    Status = Status.BusinessException,
                    ErrorMessage = ex.Message
                };
            }

        }
    }
}
