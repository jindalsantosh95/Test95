using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.PromotionSearch;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Application.Command.PromotionCampaignSearch
{
    public class GetPromotionCommand : PromotionSearchRequestData, IRequest<Result<Response<PromotionSearchResponse>>>
    {
    }
}
