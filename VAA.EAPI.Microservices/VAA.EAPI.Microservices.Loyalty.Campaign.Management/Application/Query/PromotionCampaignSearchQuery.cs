using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.PromotionCampaignSearch;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Query
{
    public class PromotionCampaignSearchQuery : PromotionCampaignSearchRequestData, IRequest<Result<Response<PromotionCampaignSearchResponse>>>
    {
    }
}
