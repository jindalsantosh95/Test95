using CSharpFunctionalExtensions;
using MediatR;
using System;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Basket.Domain.Enums;

namespace VAA.EAPI.Microservices.Retail.Basket.Application.Command
{
    public class SetCoBrandStatusCommand : IRequest<Result<Response<Domain.Basket>>>
    {
        public Guid Id { get; set; }   
        public CoBrandStatusEnum CoBrandProgressStatus { get; set; } 
    }
}