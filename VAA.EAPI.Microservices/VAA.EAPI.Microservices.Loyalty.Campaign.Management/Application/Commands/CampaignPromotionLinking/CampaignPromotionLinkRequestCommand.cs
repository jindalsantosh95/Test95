using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignPromotionLinking;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Commands.CampaignPromotionLinking
{
    public class CampaignPromotionLinkRequestCommand : CampaignPromotionLinkRequestData, IRequest<Result<Response<string>>>
    {
    }
}
