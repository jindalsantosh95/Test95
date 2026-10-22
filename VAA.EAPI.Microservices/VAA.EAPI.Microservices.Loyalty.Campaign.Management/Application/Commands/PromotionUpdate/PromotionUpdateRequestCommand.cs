using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.PromotionUpdate;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Commands.PromotionUpdate
{
    public class PromotionUpdateRequestCommand : PromotionUpdateRequestData, IRequest<Result<Response<string>>>
    {
    }
}
