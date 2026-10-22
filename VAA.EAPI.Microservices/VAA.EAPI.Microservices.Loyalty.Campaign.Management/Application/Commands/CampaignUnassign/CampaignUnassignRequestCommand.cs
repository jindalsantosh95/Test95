using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignUnassign;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Commands.CampaignUnassign
{
    public class CampaignUnassignRequestCommand : CampaignUnassignRequestData, IRequest<Result<Response<string>>>
    {
    }
}


