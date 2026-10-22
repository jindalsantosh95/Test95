using CSharpFunctionalExtensions;
using MediatR;
using System;
using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.Microservices.Retail.Basket.Application.Command
{
    public sealed class DeleteBasketRequest : IRequest<Result<Response<bool>>>
    {
        public Guid Id { get; set; }
    }
}
