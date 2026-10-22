using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.PromotionCampaignSearch;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Application.Command.PromotionSearch
{
    public class GetPromotionCampaignCommand : PromotionCampaignSearchRequestData, IRequest<Result<Response<PromotionCampaignSearchResponse>>>
    {
    }
}
