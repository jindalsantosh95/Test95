using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Microservices.Loyalty.Wallet.Domain;

namespace VAA.EAPI.Microservices.Loyalty.Wallet.Application.Query
{
    public class GetPassQueryHandler : IRequestHandler<GetPassQuery, Response<AssociatedPasses>>
    {
        private readonly ITableStorageService<PassEntities> _tableStorageService;
        private readonly ILogger<GetPassQuery> _logger;

        public GetPassQueryHandler(ITableStorageService<PassEntities> tableStorageService, ILogger<GetPassQuery> logger)
        {
            _tableStorageService = tableStorageService ?? throw new ArgumentNullException(nameof(tableStorageService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Response<AssociatedPasses>> Handle(GetPassQuery request, CancellationToken cancellationToken)
        {
            try
            {
                AssociatedPasses associatedPasses = new();

                var passRegisteredData = await _tableStorageService.RetrieveItemsAsync(Constants.Application.WalletPassEvent, Constants.Application.DeviceId, request.DeviceId, default);

                if (passRegisteredData.Any())
                {
                    associatedPasses.SerialNumbers = passRegisteredData.Select(pass => pass.SerialNumber).ToArray();
                    associatedPasses.LastUpdated = passRegisteredData.First()?.LastUpdated;                                     
                }

                return new Response<AssociatedPasses>()
                {
                    Status = Status.OK,
                    Value = associatedPasses,
                    Errors = null
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while getting associated passes for deviceID: {request.DeviceId} with error {ex.Message}");

                return new Response<AssociatedPasses>()
                {
                    Status = Status.BusinessException,
                    ErrorMessage = ex.Message
                };
            }
        }
    }
}
