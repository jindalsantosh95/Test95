using CSharpFunctionalExtensions;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignRegister;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignSearch;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignTypeRetrieval;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.PromotionCampaignSearch;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.PromotionSearch;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Repositories.CommonRefDB.Interface
{
    public interface ICommonRefDBRepository
    {
        Task<CampaignDetails> GetCampaignDetails(string campaignId);
        Task<string> GetPromotionId(string campaignCode);
        Task<string> UpdateCampaign(string campaignId, string campaignCode, string startDate, string endDate, string campaignName, string description, string linkToTemplate, bool registrationRequiredFlag, string status, string type, string initiatedBy);
        Task<string> CreateCampaign(string campaignCode, string startDate, string endDate, string campaignName, string description, string linkToTemplate, bool registrationRequiredFlag, string status, string type, string initiatedBy);
        Task<string> InsertCampaignPromtionLink(string campaignId, string promotionId, string createdBy);
        Task<Result<Response<PromotionSearchResponse>>> GetPromotionSearch(string PromotionId, string PromotionName, string StartDateF, string EndDateF, string StartDateT, string EnddateT, string PartnerName, string Active);
        Task<Result<Response<CampaignTypeResponse>>> GetCampaignTypes();
        Task<string> UpdatePromotion(string promotionId, string promotionName, string partnerName, string startDate, string endDate, string active, string campaignLinked, string initiatedBy);
        Task<string> DeleteCampaignPromtionLink(string campaignId, string promotionId);
        Task<string> CreatePromotion(string promotionName, string partnerName, string startDate, string endDate, string active, string initiatedBy);
        Task<Result<Response<PromotionCampaignSearchResponse>>> GetPromotionCampaignSearch(string PromotionId);
        Task<Result<Response<CampaignSearchResponse>>> SearchCampaigns(string campaignId, string campaignName, string startDateF, string endDateF, string startDateT, string endDateT, string campaignCode, string status, string type);
    }
}
