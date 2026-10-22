using CSharpFunctionalExtensions;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignRegister;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignSearch;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignTypeRetrieval;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.PromotionCampaignSearch;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.PromotionSearch;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Mapper;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Repositories.CommonRefDB.Interface;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Services.Interface;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Repositories.CommonRefDB
{
    public class CommonRefDBRepository : ICommonRefDBRepository
    {
        private readonly ICommonRefDBServices _commonRefDBServices;
        public CommonRefDBRepository(ICommonRefDBServices commonRefDBServices)
        {
            _commonRefDBServices = commonRefDBServices ?? throw new ArgumentNullException(nameof(commonRefDBServices));
        }
        public async Task<CampaignDetails> GetCampaignDetails(string campaignId)
        {
            CampaignDetails campaignDetails = CampaignDetailMapper.MapCamppaignData(await _commonRefDBServices.GetAllCampaignDetails(campaignId));
            return campaignDetails;
        }
        public async Task<string> UpdateCampaign(string campaignId, string campaignCode, string startDate, string endDate, string campaignName, string description, string linkToTemplate, bool registrationRequiredFlag, string status, string type, string initiatedBy)
        {
            return await _commonRefDBServices.UpdateCampaignDetails(campaignId, campaignCode, startDate, endDate, campaignName, description, linkToTemplate, registrationRequiredFlag, status, type, initiatedBy);
        }
        public async Task<string> GetPromotionId(string campaignID)
        {
            return await _commonRefDBServices.GetPromotionsDetail(campaignID);
        }
        public async Task<string> CreateCampaign(string campaignCode, string startDate, string endDate, string campaignName, string description, string linkToTemplate, bool registrationRequiredFlag, string status, string type, string initiatedBy)
        {
            return await _commonRefDBServices.CreateNewCampaign(campaignCode, startDate, endDate, campaignName, description, linkToTemplate, registrationRequiredFlag, status, type, initiatedBy);
        }
        public async Task<string> InsertCampaignPromtionLink(string campaignId, string promotionId, string createdBy)
        {
            return await _commonRefDBServices.InsertCampaignPromtionLink(campaignId, promotionId, createdBy);
        }
        public async Task<Result<Response<CampaignTypeResponse>>> GetCampaignTypes()
        {
            return await _commonRefDBServices.GetCampaignTypeList();
        }
        public async Task<string> UpdatePromotion(string promotionId, string promotionName, string partnerName, string startDate, string endDate, string active, string campaignLinked, string initiatedBy)
        {
            return await _commonRefDBServices.UpdatePromotionDetails(promotionId, promotionName, partnerName, startDate, endDate, active, campaignLinked, initiatedBy);
        }

        public async Task<Result<Response<PromotionSearchResponse>>> GetPromotionSearch(string PromotionId, string PromotionName, string StartDateF, string EndDateF, string StartDateT, string EnddateT, string PartnerName, string Active)
        {
            return await _commonRefDBServices.GetPromotionSearchList(PromotionId, PromotionName, StartDateF, EndDateF, StartDateT, EnddateT, PartnerName, Active);
        }

        public async Task<string> DeleteCampaignPromtionLink(string campaignId, string promotionId)
        {
            return await _commonRefDBServices.DeleteCampaignPromtionLink(campaignId, promotionId);
        }
        public async Task<string> CreatePromotion(string promotionName, string partnerName, string startDate, string endDate, string active, string initiatedBy)
        {
            return await _commonRefDBServices.CreateNewPromotion(promotionName, partnerName, startDate, endDate, active, initiatedBy);
        }

        public async Task<Result<Response<PromotionCampaignSearchResponse>>> GetPromotionCampaignSearch(string PromotionId)
        {
            return await _commonRefDBServices.GetPromotionCampaignSearchList(PromotionId);
        }

        public async Task<Result<Response<CampaignSearchResponse>>> SearchCampaigns(string campaignId, string campaignName, string startDateF, string endDateF, string startDateT, string endDateT, string campaignCode, string status, string type)
        {
            return await _commonRefDBServices.GetCampaignSearchList(campaignId, campaignName, startDateF, endDateF, startDateT, endDateT, campaignCode, status, type);
        }
    }
}
