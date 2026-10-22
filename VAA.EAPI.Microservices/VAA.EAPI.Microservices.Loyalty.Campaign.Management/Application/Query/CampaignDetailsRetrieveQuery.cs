using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignSearch;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Query
{
    public class CampaignDetailsRetrieveQuery : CampaignSearchRequestData, IRequest<Result<Response<CampaignRetrieveResponse>>>
    {
    }
}
