using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignRegister;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignRetrieval;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Repositories.CampaignRetrieve.Interface;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Repositories.CommonRefDB.Interface;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Services.Interface;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Mapper.CampaignRetrieval;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Repositories.CampaignRetrieve
{
    public class CampaignRetrieveRepository : ICampaignRetrieveRepository
    {
        private readonly ITargetedMemberDocumentService _targetedMemberDocumentService;
        private readonly ILogger<CampaignRetrieveRepository> _logger;
        private readonly ICommonRefDBRepository _commonRefDBRepository;
        public CampaignRetrieveRepository(ICommonRefDBRepository commonRefDBRepository, ITargetedMemberDocumentService targetedMemberDocumentService, ILogger<CampaignRetrieveRepository> logger)
        {
            _commonRefDBRepository = commonRefDBRepository ?? throw new ArgumentNullException(nameof(commonRefDBRepository));
            _targetedMemberDocumentService = targetedMemberDocumentService ?? throw new ArgumentNullException(nameof(targetedMemberDocumentService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<Result<Response<MemberCampaignResponse>>> GetMemberCampaigns(string customerId)
        {
            List<CampaignDetails> campaignDetails = new List<CampaignDetails>();
            var targatedresponse = await _targetedMemberDocumentService.GetMemberCampaigns(customerId);
            if (targatedresponse != null)
            {
                var targetedMemberships = targatedresponse.TargetedMembershipDocuments;
                foreach (var item in targetedMemberships)
                {
                    try
                    {
                        var campaignDetail = await _commonRefDBRepository.GetCampaignDetails(item.ProfilingCampaignSourceId);
                        campaignDetails.Add(campaignDetail);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error has occured in campaign reference data retrieval for campaign id {item.ProfilingCampaignSourceId}");
                    }
                }
                var response = CampaignRetrievalResponseMapping.ResponseMapping(targetedMemberships, campaignDetails);
                _logger.LogInformation("Get Member Campaign Retrieve Success");
                return new Response<MemberCampaignResponse>
                {
                    Status = Status.OK,
                    Value = response
                };
            }
            else
            {
                _logger.LogInformation("Get Member Campaign Retrieve Failed");
                return Result.Failure<Response<MemberCampaignResponse>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
        }
    }
}
