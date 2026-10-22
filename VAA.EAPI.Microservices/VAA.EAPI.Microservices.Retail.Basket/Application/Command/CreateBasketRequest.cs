using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.Microservices.Retail.Basket.Application.Command
{
    public sealed class CreateBasketRequest : IRequest<Result<Response<Domain.Basket>>>
    {
        public Domain.Basket Basket { get; set; }
    }
}