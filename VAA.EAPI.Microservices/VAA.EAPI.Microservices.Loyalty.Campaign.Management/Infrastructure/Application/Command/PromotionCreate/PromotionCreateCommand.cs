using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.PromotionCreate;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Application.Command.PromotionCreate
{
    public class PromotionCreateCommand : PromotionCreateRequestData, IRequest<Result<Response<string>>>
    {
    }
}
