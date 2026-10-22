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
    public class MemberProfileRepository : IMemberProfileRepository
    {
        public ICustomerTransactionService<MemberProfileDTO> _customerTransactionService;
        public readonly ILogger<MemberProfileRepository> _logger;

        public MemberProfileRepository(ICustomerTransactionService<MemberProfileDTO> customerTransactionService, ILogger<MemberProfileRepository> logger)
        {
            _customerTransactionService = customerTransactionService ?? throw new ArgumentNullException(nameof(customerTransactionService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task AddMemberProfileDetails(CreateCustomerEntities createCustomerEntities)
        {
            _logger.LogInformation($"SCVPlus - AddMemberProfileDetails Start");

            try
            {
                MemberProfile SCVPlusMemberProfileDetails = MemberProfileMapper.MapEAPIMemberProfileToSCVMemberProfile(createCustomerEntities, DocumentType.FlyingClubMember.ToString());
                await _customerTransactionService.CreateItemAsync(new MemberProfileDTO(SCVPlusMemberProfileDetails), SCVPlusMemberProfileDetails?.PartitionKey, DocumentType.FlyingClubMember);
                _logger.LogDebug($"AddMemberProfileDetails success for Customer ID :{createCustomerEntities?.CustomerId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"AddMemberProfileDetails failed for Customer ID :{createCustomerEntities?.CustomerId} with message :{ex.Message}");
                throw;
            }
            finally
            {
                _logger.LogDebug($"SCVPlus - AddMemberProfileDetails End");
            }
        }
        public async Task UpdateMemberProfileDetails(CreateCustomerEntities createCustomerEntities, MemberProfile customers)
        {
            _logger.LogInformation($"SCVPlus - UpdateMemberProfileDetails Start");

            try
            {
                MemberProfile SCVPlusMemberProfileDetails = MemberProfileMapper.MapEAPIMemberProfileToSCVMemberProfile(createCustomerEntities, DocumentType.FlyingClubMember.ToString());

                //Preserve all createdby and CreatedOn before replacing existing docs in Cosmos
                SCVPlusMemberProfileDetails.CreatedOn = DateTime.ParseExact(customers.CreatedOn, Constants.Application.DatetimeFormat, new System.Globalization.CultureInfo(Constants.Application.gbCulture)).ToString(Constants.Application.ISODateTimeFormat);
                SCVPlusMemberProfileDetails.CreatedBy = customers.CreatedBy;
                SCVPlusMemberProfileDetails.MemberShipCardDetails.CreatedBy = customers.CreatedBy;
                SCVPlusMemberProfileDetails.MemberShipCardDetails.CreatedOn = DateTime.ParseExact(customers.CreatedOn, Constants.Application.DatetimeFormat, new System.Globalization.CultureInfo(Constants.Application.gbCulture)).ToString(Constants.Application.ISODateTimeFormat);
                SCVPlusMemberProfileDetails.MemberShipCardDetails.CardId = customers.MemberShipCardDetails.CardId;
                foreach (var x in SCVPlusMemberProfileDetails.MemberAttributes)
                {
                    x.CreatedOn = DateTime.ParseExact(customers.CreatedOn, Constants.Application.DatetimeFormat, new System.Globalization.CultureInfo(Constants.Application.gbCulture)).ToString(Constants.Application.ISODateTimeFormat);
                    x.CreatedBy = customers.CreatedBy;

                }

                await _customerTransactionService.ReplaceItemAsync(new MemberProfileDTO(SCVPlusMemberProfileDetails), SCVPlusMemberProfileDetails?.PartitionKey, DocumentType.FlyingClubMember);
                _logger.LogDebug($"UpdateMemberProfileDetails success for Customer ID :{createCustomerEntities?.CustomerId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"UpdateMemberProfileDetails failed for Customer ID :{createCustomerEntities?.CustomerId} with message :{ex.Message}");
                throw;
            }
            finally
            {
                _logger.LogDebug($"SCVPlus - UpdateMemberProfileDetails End");
            }
        }
    }
}
