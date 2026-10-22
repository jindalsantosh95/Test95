using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.PromotionsDownload;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Query
{
    public class PromotionDownloadQuery : IRequest<Result<Response<PromotionsDownloadResponse>>>
    {
    }
}
