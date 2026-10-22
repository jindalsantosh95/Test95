using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Interfaces.EnrolmentCustomer;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.CustomerDoc
{
    public class CustomerRepoUpdateCommandHandler : IRequestHandler<CustomerRepoUpdateCommand, Result<Unit>>
    {
        public ICustomerService<CustomersDTO> _customerService;
        public readonly ILogger<CustomerRepoUpdateCommandHandler> _logger;

        public CustomerRepoUpdateCommandHandler(ICustomerService<CustomersDTO> customerService, ILogger<CustomerRepoUpdateCommandHandler> logger)
        {
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Unit>> Handle(CustomerRepoUpdateCommand request, CancellationToken cancellationToken)
        {
            try
            {
                await _customerService.ReplaceItemAsync(new CustomersDTO(request.CustomerDocument), DocumentType.Customer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"UpdateCustomerSearch failed for MemberNumber :{request?.CustomerDocument?.LoyaltyMemberNumber} with message :{ex.Message}");
                throw;
            }

            return Unit.Value;
        }
    }
}