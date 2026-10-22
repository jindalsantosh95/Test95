using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignUnassign;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Query
{
    public class CampaignUnassignResponseQuery : IRequest<Result<Response<CampaignUnassignResponse>>>
    {
        public string CorrelationId { get; set; }
        public string Type { get; set; }
    }
}

