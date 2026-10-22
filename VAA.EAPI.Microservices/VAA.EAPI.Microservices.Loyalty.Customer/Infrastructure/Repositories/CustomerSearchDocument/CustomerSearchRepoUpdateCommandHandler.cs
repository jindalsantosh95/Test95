using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Interfaces.EnrolmentCustomer;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.CustomerSearchDocument
{
    public class CustomerSearchRepoUpdateCommandHandler : IRequestHandler<CustomerSearchRepoUpdateCommand, Result<Unit>>
    {
        public ICustomerService<CustomerSearchDTO> _customerService;
        public readonly ILogger<CustomerSearchRepoUpdateCommandHandler> _logger;

        public CustomerSearchRepoUpdateCommandHandler(ICustomerService<CustomerSearchDTO> customerService, ILogger<CustomerSearchRepoUpdateCommandHandler> logger)
        {
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Unit>> Handle(CustomerSearchRepoUpdateCommand request, CancellationToken cancellationToken)
        {
            try
            {
                await _customerService.ReplaceItemAsync(new CustomerSearchDTO(request.CustomerSearchDocument), DocumentType.CustomerSearch);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"UpdateCustomerSearch failed for MemberNumber :{request?.CustomerSearchDocument?.LoyaltyMemberNumber} with message :{ex.Message}");
                throw;
            }

            return Unit.Value;
        }
    }
}