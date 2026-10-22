using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignCreate;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Commands.CampaignCreate
{
    public class CampaignCreateRequestCommand : CampaignCreateRequestData, IRequest<Result<Response<string>>>
    {
    }
}
