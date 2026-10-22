using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Models;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Application.Command.CampaignPromotionLinking
{
    public class InsertCampaignPromotionLinkCommand : CampaignsPromotionsMapping, IRequest<Result<Response<string>>>
    {
    }
}
