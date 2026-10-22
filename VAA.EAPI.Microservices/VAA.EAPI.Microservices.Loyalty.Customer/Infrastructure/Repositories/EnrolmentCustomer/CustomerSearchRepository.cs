using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.IRepository;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Interfaces.EnrolmentCustomer;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Mapper.EnrolmentCustomer;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.EnrolmentCustomer
{
    public class CustomerSearchRepository : ICustomerSearchRepository
    {
        public ICustomerService<CustomerSearchDTO> _customerService;
        public readonly ILogger<CustomerSearchRepository> _logger;

        public CustomerSearchRepository(ICustomerService<CustomerSearchDTO> customerService, ILogger<CustomerSearchRepository> logger)
        {
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task AddCustomerSearchDetails(CreateCustomerEntities createCustomerEntities)
        {
            _logger.LogInformation($"SCVPlus - AddCustomerSearchDetails Start");

            try
            {
                CustomerSearch customerSearchDetails = CustomerSearchMapper.MapEAPICustomerToSCVCustomerSearch(createCustomerEntities, DocumentType.CustomerSearch.ToString());
                await _customerService.CreateItemAsync(new CustomerSearchDTO(customerSearchDetails), customerSearchDetails?.PartitionKey, DocumentType.CustomerSearch);
                _logger.LogDebug($"AddCustomerSearchDetails success for Customer ID :{createCustomerEntities?.CustomerId}");
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, $"AddCustomerSearchDetails failed for Customer ID :{createCustomerEntities?.CustomerId} with message :{ex.Message}");
                throw;
            }
            finally
            {
                _logger.LogDebug($"SCVPlus - AddCustomerSearchDetails End");
            }
        }
        public async Task UpdateCustomerSearchDetails(CreateCustomerEntities createCustomerEntities, Customers customers)
        {
            _logger.LogInformation($"SCVPlus - UpdateCustomerSearchDetails Start");

            try
            {
                CustomerSearch customerSearchDetails = CustomerSearchMapper.MapEAPICustomerToSCVCustomerSearch(createCustomerEntities, DocumentType.CustomerSearch.ToString());
                await _customerService.ReplaceItemAsync(new CustomerSearchDTO(customerSearchDetails), DocumentType.CustomerSearch);
                _logger.LogDebug($"UpdateCustomerSearchDetails success for Customer ID :{createCustomerEntities?.CustomerId}");
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, $"UpdateCustomerSearchDetails failed for Customer ID :{createCustomerEntities?.CustomerId} with message :{ex.Message}");
                throw;
            }
            finally
            {
                _logger.LogDebug($"SCVPlus - UpdateCustomerSearchDetails End");
            }
        }
    }
}
