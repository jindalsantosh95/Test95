using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.PromotionUpdate;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Application.Command.PromotionUpdate
{
    public class PromotionUpdateCommand : PromotionUpdateRequestData, IRequest<Result<Response<string>>>
    {
    }
}
