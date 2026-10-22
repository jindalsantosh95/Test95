using CSharpFunctionalExtensions;
using MediatR;
using System;
using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.Microservices.Retail.Basket.Application.Query
{
    public sealed class LoadBasketRequest : IRequest<Result<Response<Domain.Basket>>>
    {
        public Guid Id { get; set; }
    }
}
