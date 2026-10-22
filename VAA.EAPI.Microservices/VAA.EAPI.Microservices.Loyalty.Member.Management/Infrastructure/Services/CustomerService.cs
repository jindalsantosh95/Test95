using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Customer;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Models;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.Interface;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services
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
            try
            {
                _logger.LogInformation($"Get Customer Details service with Customer id or MemberNumber started");
                var resp = await _mediator.Send(request: new CustomerDetailsServiceQuery { CustomerId = customerId, MemberId = memberNumber });
                return resp.Value.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, $"Error has occured in customer details service call");
                throw;
            }
            finally
            {
                _logger.LogInformation($"Get Customer Details service ended");
            }
        }

        public async Task<Customer> GetCustomerDocument(string customerId)
        {
            try
            {
                var resp = await _mediator.Send(request: new CustomerDocumentServiceQuery { CustomerId = customerId });
                return resp.Value.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, $"Error has occured in customer document retrieval service call");
                throw;
            }
        }

        public async Task<CustomerSearch> GetCustomerSearchDocument(string memberNumber)
        {
            try
            {
                var resp = await _mediator.Send(request: new CustomerSearchDocumentServiceQuery { MemberNumber = memberNumber });
                return resp.Value.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, $"Error has occured in customer search document retrieval service call");
                throw;
            }
        }
    }
}
