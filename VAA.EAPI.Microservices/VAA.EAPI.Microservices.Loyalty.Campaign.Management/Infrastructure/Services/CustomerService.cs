using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Models;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Services.Interface;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Services
{
    public class CustomerService : ICustomerProvider
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CustomerService> _logger;

        public CustomerService(IMediator mediator, ILogger<CustomerService> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<CustomerDTO> GetCustomer(string customerId, string memberNumber)
        {
            var resp = await _mediator.Send(request: new CustomerDetailsServiceQuery { CustomerId = customerId, MemberNumber = memberNumber });
            return resp.Value.Value;
        }
    }
}
