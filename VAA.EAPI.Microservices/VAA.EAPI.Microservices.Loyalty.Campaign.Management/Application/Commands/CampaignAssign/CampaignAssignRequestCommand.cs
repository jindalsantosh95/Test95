using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignAssign;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Commands.CampaignAssign
{
    public class CampaignAssignRequestCommand : CamapignAssignRequestData, IRequest<Result<Response<string>>>
    {
    }
}
