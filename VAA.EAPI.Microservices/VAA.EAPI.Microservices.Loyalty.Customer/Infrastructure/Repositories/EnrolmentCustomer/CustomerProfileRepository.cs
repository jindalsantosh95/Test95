using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.IRepository;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Interfaces.EnrolmentCustomer;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Mapper.EnrolmentCustomer;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.EnrolmentCustomer
{
    public class CustomerProfileRepository : ICustomerProfileRepository
    {
        public ICustomerTransactionService<CustomerProfileDTO> _customerTransactionService;
        public readonly ILogger<CustomerProfileRepository> _logger;

        public CustomerProfileRepository(ICustomerTransactionService<CustomerProfileDTO> customerTransactionService, ILogger<CustomerProfileRepository> logger)
        {
            _customerTransactionService = customerTransactionService ?? throw new ArgumentNullException(nameof(customerTransactionService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task AddCustomerProfileDetails(CreateCustomerEntities createCustomerEntities)
        {
            _logger.LogInformation($"SCVPlus - AddCustomerProfileDetails Start");

            try
            {
                CustomerProfile SCVPlusCustomerProfileDetails = CustomerProfileMapper.MapEAPICustomerProfileToSCVCustomerProfile(createCustomerEntities, DocumentType.CustomerProfile.ToString());
                await _customerTransactionService.CreateItemAsync(new CustomerProfileDTO(SCVPlusCustomerProfileDetails), SCVPlusCustomerProfileDetails?.PartitionKey, DocumentType.CustomerProfile);
                _logger.LogDebug($"AddCustomerProfileDetails success for Customer ID :{createCustomerEntities?.CustomerId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"AddCustomerProfileDetails failed for Customer ID :{createCustomerEntities?.CustomerId} with message {ex.Message}");
                throw;
            }
            finally
            {
                _logger.LogDebug($"SCVPlus - AddCustomerProfileDetails End");
            }
        }
        public async Task UpdateCustomerProfileDetails(CreateCustomerEntities createCustomerEntities, CustomerProfile customers)
        {
            _logger.LogInformation($"SCVPlus - UpdateCustomerProfileDetails Start");

            try
            {
                CustomerProfile SCVPlusCustomerProfileDetails = CustomerProfileMapper.MapEAPICustomerProfileToSCVCustomerProfile(createCustomerEntities, DocumentType.CustomerProfile.ToString());

                //Preserve all createdby and CreatedOn before replacing existing docs in Cosmos
                SCVPlusCustomerProfileDetails.CreatedOn = DateTime.ParseExact(customers.CreatedOn, Constants.Application.DatetimeFormat, new System.Globalization.CultureInfo(Constants.Application.gbCulture)).ToString(Constants.Application.ISODateTimeFormat);

                SCVPlusCustomerProfileDetails.CreatedBy = customers.CreatedBy;
                foreach (var x in SCVPlusCustomerProfileDetails.Preferences)
                {
                    x.PreferenceId = customers.Preferences.FirstOrDefault(p => p.PreferenceType == x.PreferenceType) != null ? customers.Preferences.FirstOrDefault(p => p.PreferenceType == x.PreferenceType).PreferenceId : x.PreferenceId;
                    x.CreatedOn = DateTime.ParseExact(customers.CreatedOn, Constants.Application.DatetimeFormat, new System.Globalization.CultureInfo(Constants.Application.gbCulture)).ToString(Constants.Application.ISODateTimeFormat);
                    x.CreatedBy = customers.CreatedBy;
                }

                await _customerTransactionService.ReplaceItemAsync(new CustomerProfileDTO(SCVPlusCustomerProfileDetails), SCVPlusCustomerProfileDetails?.PartitionKey, DocumentType.CustomerProfile);
                _logger.LogDebug($"UpdateCustomerProfileDetails success for Customer ID :{createCustomerEntities?.CustomerId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"UpdateCustomerProfileDetails failed for Customer ID :{createCustomerEntities?.CustomerId} with message {ex.Message}");
                throw;
            }
            finally
            {
                _logger.LogDebug($"SCVPlus - UpdateCustomerProfileDetails End");
            }
        }
    }
}
