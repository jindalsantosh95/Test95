using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.PromotionCreate;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Commands.PromotionCreate
{
    public class PromotionCreateRequestCommand : PromotionCreateRequestData, IRequest<Result<Response<string>>>
    {
    }
}
