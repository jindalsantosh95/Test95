using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignPromotionDelinking;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Commands.CampaignPromotionDelinking
{
    public class CampaignPromotionDelinkRequestCommand : CampaignPromotionDelinkRequestData, IRequest<Result<Response<string>>>
    {
    }
}
