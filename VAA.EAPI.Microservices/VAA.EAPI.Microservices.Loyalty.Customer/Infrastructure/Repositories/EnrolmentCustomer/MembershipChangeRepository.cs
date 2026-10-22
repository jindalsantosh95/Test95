using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.IRepository;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Interfaces.EnrolmentCustomer;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Mapper.EnrolmentCustomer;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.EnrolmentCustomer
{
    public class MembershipChangeRepository : IMembershipChangeRepository
    {
        public ICustomerTransactionService<MembershipChangeDTO> _customerTransactionService;
        public readonly ILogger<MembershipChangeRepository> _logger;

        public MembershipChangeRepository(ICustomerTransactionService<MembershipChangeDTO> customerTransactionService, ILogger<MembershipChangeRepository> logger)
        {
            _customerTransactionService = customerTransactionService ?? throw new ArgumentNullException(nameof(customerTransactionService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<Result<Response<TierInfo>>> AddMembershipChangeDetails(CreateCustomerEntities createCustomerEntities)
        {
            _logger.LogInformation($"SCVPlus - AddMembershipChangeDetails Start");

            Response<TierInfo> tierDetails = new Response<TierInfo>();
            try
            {
                MembershipChange SCVPlusMembershipChangeDetails = MembershipChangeMapper.MapEAPIMembershipChangeToSCVMemberMembershipChange(DocumentType.MembershipChange.ToString(), createCustomerEntities);
                await _customerTransactionService.CreateItemAsync(new MembershipChangeDTO(SCVPlusMembershipChangeDetails), SCVPlusMembershipChangeDetails?.PartitionKey, DocumentType.MembershipChange);
                tierDetails.Value = TierDetailMapper.MapFromSCVResponse(createCustomerEntities, SCVPlusMembershipChangeDetails);
                _logger.LogDebug($"AddMembershipChangeDetails success for Customer ID :{createCustomerEntities?.CustomerId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"AddMembershipChangeDetails failed for Customer ID :{createCustomerEntities?.CustomerId} with message :{ex.Message}");
                throw;
            }
            finally
            {
                _logger.LogDebug($"SCVPlus - AddMembershipChangeDetails End");
            }

            return tierDetails;
        }
        public async Task<Result<Response<TierInfo>>> UpdateMembershipChangeDetails(CreateCustomerEntities createCustomerEntities, MembershipChange membershipChange)
        {
            _logger.LogInformation($"SCVPlus - UpdateMembershipChangeDetails Start");

            Response<TierInfo> tierDetails = new Response<TierInfo>();
            try
            {
                MembershipChange SCVPlusMembershipChangeDetails = MembershipChangeMapper.MapEAPIMembershipChangeToSCVMemberMembershipChange(DocumentType.MembershipChange.ToString(), createCustomerEntities);

                //Preserve Id, createdby and CreatedOn before replacing existing docs in Cosmos
                SCVPlusMembershipChangeDetails.CreatedOn = DateTime.ParseExact(membershipChange.CreatedOn, Constants.Application.DatetimeFormat, new System.Globalization.CultureInfo(Constants.Application.gbCulture)).ToString(Constants.Application.ISODateTimeFormat);
                SCVPlusMembershipChangeDetails.CreatedBy = membershipChange.CreatedBy;
                SCVPlusMembershipChangeDetails.Id = membershipChange.Id;


                await _customerTransactionService.ReplaceItemAsync(new MembershipChangeDTO(SCVPlusMembershipChangeDetails), SCVPlusMembershipChangeDetails?.PartitionKey, DocumentType.MembershipChange);
                tierDetails.Value = TierDetailMapper.MapFromSCVResponse(createCustomerEntities, SCVPlusMembershipChangeDetails);
                _logger.LogDebug($"UpdateMembershipChangeDetails success for Customer ID :{createCustomerEntities?.CustomerId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"UpdateMembershipChangeDetails failed for Customer ID :{createCustomerEntities?.CustomerId} with message :{ex.Message}");
                throw;
            }
            finally
            {
                _logger.LogDebug($"SCVPlus - UpdateMembershipChangeDetails End");
            }

            return tierDetails;
        }
    }
}
