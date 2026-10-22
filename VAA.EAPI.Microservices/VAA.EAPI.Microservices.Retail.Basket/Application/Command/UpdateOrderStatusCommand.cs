using CSharpFunctionalExtensions;
using MediatR;
using System;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Basket.Domain.Enums;

namespace VAA.EAPI.Microservices.Retail.Basket.Application.Command
{
    public class UpdateOrderStatusCommand : IRequest<Result<Response<Domain.Basket>>>
    {
        public Guid Id { get; set; }
        public BasketStatusEnum Status { get; set; }

    }
}
