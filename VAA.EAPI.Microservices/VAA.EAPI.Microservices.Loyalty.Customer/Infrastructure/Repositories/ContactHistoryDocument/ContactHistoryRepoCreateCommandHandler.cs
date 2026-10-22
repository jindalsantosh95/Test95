using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Interfaces.EnrolmentCustomer;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.ContactHistoryDocument
{
    public class ContactHistoryRepoCreateCommandHandler : IRequestHandler<ContactHistoryRepoCreateCommand, Result<Unit>>
    {
        public ICustomerService<ContactHistoryDTO> _customerService;
        public readonly ILogger<ContactHistoryRepoCreateCommandHandler> _logger;

        public ContactHistoryRepoCreateCommandHandler(ICustomerService<ContactHistoryDTO> customerService, ILogger<ContactHistoryRepoCreateCommandHandler> logger)
        {
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Unit>> Handle(ContactHistoryRepoCreateCommand request, CancellationToken cancellationToken)
        {
            try
            {
                await _customerService.CreateItemAsync((ContactHistoryDTO)request.ContactHistoryEntities, request?.ContactHistoryEntities?.CustomerId, DocumentType.ContactHistory);
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, $"Create ContactHistory failed for CustomerId :{request?.ContactHistoryEntities?.CustomerId} with message :{ex.Message}");
                throw;
            }

            return Unit.Value;
        }
    }
}
