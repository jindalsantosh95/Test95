using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignUpdate;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Commands.CampaignUpdate
{
    public class CampaignUpdateRequestCommand : CampaignUpdateRequestData, IRequest<Result<Response<string>>>
    {
    }
}
