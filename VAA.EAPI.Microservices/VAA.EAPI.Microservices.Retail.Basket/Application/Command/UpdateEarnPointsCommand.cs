using CSharpFunctionalExtensions;
using MediatR;
using System;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Basket.Domain.FCEarnPoints;

namespace VAA.EAPI.Microservices.Retail.Basket.Application.Command
{
    public class UpdateEarnPointsCommand : IRequest<Result<Response<Domain.Basket>>>
    {
        public Guid Id { get; set; }
        public EarnPoints EarnPoints { get; set; }
    }
}