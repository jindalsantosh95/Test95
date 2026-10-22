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
    public class PartnerDocumentRepository : IPartnerDocumentRepository
    {
        public ICustomerTransactionService<PartnerMembershipDTO> _customerTransactionService;
        public readonly ILogger<PartnerDocumentRepository> _logger;


        public PartnerDocumentRepository(ICustomerTransactionService<PartnerMembershipDTO> customerTransactionService, ILogger<PartnerDocumentRepository> logger)
        {
            _customerTransactionService = customerTransactionService ?? throw new ArgumentNullException(nameof(customerTransactionService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task AddPartnerDetails(CreateCustomerEntities createCustomerEntities)
        {
            _logger.LogInformation($"SCVPlus - AddPartnerDetails Start");

            try
            {
                PartnerMembership PartnerProfileDetails = PartnerDocumentMapper.MapPartnerDocument(createCustomerEntities, DocumentType.PartnerMembership.ToString());
                await _customerTransactionService.CreateItemAsync(new PartnerMembershipDTO(PartnerProfileDetails), PartnerProfileDetails?.PartitionKey, DocumentType.PartnerMembership);
                _logger.LogDebug($"AddPartnerDetails success for Customer ID :{createCustomerEntities?.CustomerId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"AddPartnerDetails failed for Customer ID :{createCustomerEntities?.CustomerId} with message :{ex.Message}");
                throw;
            }
            finally
            {
                _logger.LogDebug($"SCVPlus - AddPartnerDetails End");
            }
        }
        public async Task UpdatePartnerDetails(CreateCustomerEntities createCustomerEntities, PartnerMembership partnerMembership)
        {
            _logger.LogInformation($"SCVPlus - UpdatePartnerDetails Start");

            try
            {
                PartnerMembership PartnerProfileDetails = PartnerDocumentMapper.MapPartnerDocument(createCustomerEntities, DocumentType.PartnerMembership.ToString());

                //Preserve Id, createdby and CreatedOn before replacing existing docs in Cosmos
                PartnerProfileDetails.CreatedOn = DateTime.ParseExact(partnerMembership.CreatedOn, Constants.Application.DatetimeFormat, new System.Globalization.CultureInfo(Constants.Application.gbCulture)).ToString(Constants.Application.ISODateTimeFormat);
                PartnerProfileDetails.CreatedBy = partnerMembership.CreatedBy;
                PartnerProfileDetails.Id = partnerMembership.Id;
                await _customerTransactionService.ReplaceItemAsync(new PartnerMembershipDTO(PartnerProfileDetails), PartnerProfileDetails?.PartitionKey, DocumentType.PartnerMembership);
                _logger.LogDebug($"UpdatePartnerDetails success for Customer ID :{createCustomerEntities?.CustomerId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"UpdatePartnerDetails failed for Customer ID :{createCustomerEntities?.CustomerId} with message :{ex.Message}");
                throw;
            }
            finally
            {
                _logger.LogDebug($"SCVPlus - UpdatePartnerDetails End");
            }
        }
    }
}
