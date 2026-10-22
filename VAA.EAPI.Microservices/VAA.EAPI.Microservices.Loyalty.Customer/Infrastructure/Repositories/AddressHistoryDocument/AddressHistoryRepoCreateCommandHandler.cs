using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Interfaces.EnrolmentCustomer;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.AddressHistoryDocument
{
    public class AddressHistoryRepoCreateCommandHandler : IRequestHandler<AddressHistoryRepoCreateCommand, Result<Unit>>
    {
        public ICustomerService<AddressHistoryDTO> _customerService;
        public readonly ILogger<AddressHistoryRepoCreateCommandHandler> _logger;

        public AddressHistoryRepoCreateCommandHandler(ICustomerService<AddressHistoryDTO> customerService, ILogger<AddressHistoryRepoCreateCommandHandler> logger)
        {
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Unit>> Handle(AddressHistoryRepoCreateCommand request, CancellationToken cancellationToken)
        {
            try
            {
                await _customerService.CreateItemAsync(new AddressHistoryDTO(request.AddressHistory), request?.AddressHistory?.CustomerId, DocumentType.AddressHistory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"CreateAddressHistory failed for CustomerId :{request?.AddressHistory?.CustomerId} with message :{ex.Message}");
                throw;
            }

            return Unit.Value;
        }
    }
}
