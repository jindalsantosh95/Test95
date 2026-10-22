using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignsDownload;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Query
{
    public class AssociatedMembersDownloadQuery : IRequest<Result<Response<AssociatedMembersDownloadResponse>>>
    {
        public string CampaignCode { get; set; }
    }
}

