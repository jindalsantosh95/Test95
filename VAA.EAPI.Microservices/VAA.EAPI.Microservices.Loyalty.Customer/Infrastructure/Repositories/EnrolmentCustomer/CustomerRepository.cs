using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.IRepository;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Interfaces.EnrolmentCustomer;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Mapper.EnrolmentCustomer;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.EnrolmentCustomer
{
    public class CustomerRepository : ICustomerRepository
    {
        public ICustomerService<CustomersDTO> _customerService;
        public readonly ILogger<CustomerRepository> _logger;

        public CustomerRepository(ICustomerService<CustomersDTO> customerService, ILogger<CustomerRepository> logger)
        {
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        }
        public async Task AddCustomerDetails(CreateCustomerEntities createCustomerEntities)
        {
            _logger.LogInformation($"SCVPlus - AddCustomerDetails Start");
            string authCustId = string.Empty;

            try
            {
                if (!string.IsNullOrEmpty(createCustomerEntities.GuardianDetails?.CardNumber))
                {
                    var result = _customerService.GetItemAsync(DocumentType.CustomerSearch, Constants.Application.FCMemberNumQuery, createCustomerEntities.GuardianDetails.CardNumber);
                    authCustId = result.Result?.FirstOrDefault()?.CustomerId ?? Utility.GetId();
                }
                else
                {
                    authCustId = Utility.GetId();
                }
                Customers SCVPlusCustomerDetails = CustomerMapper.MapEAPICustomerToSCVCustomer(createCustomerEntities, DocumentType.Customer.ToString(), authCustId);
                await _customerService.CreateItemAsync(new CustomersDTO(SCVPlusCustomerDetails), SCVPlusCustomerDetails?.PartitionKey, DocumentType.Customer);
                _logger.LogDebug($"AddCustomerDetails success for Customer ID :{createCustomerEntities?.CustomerId}");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"AddCustomerDetails failed for Customer ID :{createCustomerEntities?.CustomerId} with message: {ex.ToString()}");
                throw;
            }
            finally
            {
                _logger.LogDebug($"SCVPlus - AddCustomerDetails End");
            }
        }
        public async Task UpdateCustomerDetails(CreateCustomerEntities createCustomerEntities, Customers customers)
        {
            _logger.LogInformation($"SCVPlus - UpdateCustomerDetails Start");
            string authCustId = string.Empty;
            try
            {
                if (!string.IsNullOrEmpty(createCustomerEntities.GuardianDetails?.CardNumber))
                {
                    var result = _customerService.GetItemAsync(DocumentType.CustomerSearch, Constants.Application.FCMemberNumQuery, createCustomerEntities.GuardianDetails.CardNumber);
                    authCustId = result.Result?.FirstOrDefault()?.CustomerId ?? Utility.GetId();
                }
                else
                {
                    authCustId = Utility.GetId();
                }
                Customers SCVPlusCustomerDetails = CustomerMapper.MapEAPICustomerToSCVCustomer(createCustomerEntities, DocumentType.Customer.ToString(), authCustId);

                //Preserve all createdby and CreatedOn before replacing existing docs in Cosmos
                SCVPlusCustomerDetails.CreatedOn = DateTime.ParseExact(customers.CreatedOn, Constants.Application.DatetimeFormat, new System.Globalization.CultureInfo(Constants.Application.gbCulture)).ToString(Constants.Application.ISODateTimeFormat);
                SCVPlusCustomerDetails.CreatedBy = customers.CreatedBy;
                foreach (var x in SCVPlusCustomerDetails.ContactDetails)
                {
                    if (customers.ContactDetails.FirstOrDefault(d => d.ContactValue == x.ContactValue) != null)
                    {
                        x.ContactId = customers.ContactDetails.FirstOrDefault(d => d.ContactValue == x.ContactValue).ContactId;
                        x.CreatedBy = customers.CreatedBy;
                    }
                    else
                    {
                        x.ContactId = Utility.GetId();
                    }
                    x.CreatedOn = DateTime.ParseExact(customers.CreatedOn, Constants.Application.DatetimeFormat, new System.Globalization.CultureInfo(Constants.Application.gbCulture)).ToString(Constants.Application.ISODateTimeFormat);
                }

                await _customerService.ReplaceItemAsync(new CustomersDTO(SCVPlusCustomerDetails), DocumentType.Customer);
                _logger.LogDebug($"UpdateCustomerDetails success for Customer ID :{createCustomerEntities?.CustomerId}");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"UpdateCustomerDetails failed for Customer ID :{createCustomerEntities?.CustomerId} with message: {ex.ToString()}");
                throw;
            }
            finally
            {
                _logger.LogDebug($"SCVPlus - UpdateCustomerDetails End");
            }
        }
    }
}
