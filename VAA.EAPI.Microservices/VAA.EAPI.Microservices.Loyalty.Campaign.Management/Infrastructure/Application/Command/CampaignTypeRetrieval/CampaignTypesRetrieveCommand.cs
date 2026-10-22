using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignTypeRetrieval;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Application.Command.CampaignTypeRetrieval
{
    public class CampaignTypesRetrieveCommand : IRequest<Result<Response<CampaignTypeResponse>>>
    {
    }
}
