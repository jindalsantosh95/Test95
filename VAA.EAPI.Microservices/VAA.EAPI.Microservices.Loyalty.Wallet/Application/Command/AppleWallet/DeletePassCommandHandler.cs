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
    public class DeletePassCommandHandler : IRequestHandler<DeletePassCommand, Response<int>>
    {
        private readonly ITableStorageService<PassEntities> _tableStorageService;
        private readonly ILogger<DeletePassCommandHandler> _logger;

        public DeletePassCommandHandler(ITableStorageService<PassEntities> tableStorageService, ILogger<DeletePassCommandHandler> logger)
        {
            _tableStorageService = tableStorageService ?? throw new ArgumentNullException(nameof(tableStorageService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Response<int>> Handle(DeletePassCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var passRegisteredData = await _tableStorageService.RetrieveAsync(Constants.Application.WalletPassEvent, $"{request.DeviceId}{Constants.Application.Separator}{request.PassSerial}");

                if (passRegisteredData is not null)                
                    await _tableStorageService.DeleteAsync(passRegisteredData);
                
                return new Response<int>()
                {
                    Status = Status.OK,
                    Value = 200,
                    Errors = null
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while deleting the pass from wallet for deviceID: {request.DeviceId} with error {ex.Message}");

                return new Response<int>()
                {
                    Status = Status.BusinessException,
                    ErrorMessage = ex.Message
                };
            }

        }
    }
}
