using CSharpFunctionalExtensions;
using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignSearch;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignTypeRetrieval;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.PromotionCampaignSearch;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.PromotionSearch;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Models;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Services.Interface
{
    public interface ICommonRefDBServices
    {
        Task<Models.Campaign> GetAllCampaignDetails(string campaignId);
        Task<string> GetPromotionsDetail(string campaignId);
        Task<string> UpdateCampaignDetails(string campaignId, string campaignCode, string startDate, string endDate, string campaignName, string description, string linkToTemplate, bool registrationRequiredFlag, string status, string type, string initiatedBy);
        Task<string> CreateNewCampaign(string campaignCode, string startDate, string endDate, string campaignName, string description, string linkToTemplate, bool registrationRequiredFlag, string status, string type, string initiatedBy);
        Task<string> InsertCampaignPromtionLink(string campaignId, string promotionId, string createdBy);
        Task<Result<Response<CampaignTypeResponse>>> GetCampaignTypeList();
        Task<string> UpdatePromotionDetails(string promotionId, string promotionName, string partnerName, string startDate, string endDate, string active, string campaignLinked, string initiatedBy);
        Task<Result<Response<PromotionSearchResponse>>> GetPromotionSearchList(string PromotionId, string PromotionName, string StartDateF, string EndDateF, string StartDateT, string EnddateT, string PartnerName, string Active);
        Task<string> DeleteCampaignPromtionLink(string campaignId, string promotionId);
        Task<string> CreateNewPromotion(string promotionName, string partnerName, string startDate, string endDate, string active, string initiatedBy);
        Task<Result<Response<PromotionCampaignSearchResponse>>> GetPromotionCampaignSearchList(string PromotionId);
        Task<List<PromotionsDownload>> GetPromotionslist();
        Task<Result<Response<CampaignSearchResponse>>> GetCampaignSearchList(string campaignId, string campaignName, string startDateF, string endDateF, string startDateT, string endDateT, string campaignCode, string status, string type);
    }
}
