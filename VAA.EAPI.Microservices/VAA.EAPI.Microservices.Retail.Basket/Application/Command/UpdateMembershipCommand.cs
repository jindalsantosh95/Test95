using CSharpFunctionalExtensions;
using MediatR;
using System;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Basket.Domain;

namespace VAA.EAPI.Microservices.Retail.Basket.Application.Command
{
    public class UpdateMembershipCommand : IRequest<Result<Response<Domain.Basket>>>
    {
        public Guid Id { get; set; }
        public LoyaltyMemberDetails LoyaltyDetails { get; set; }
    }
}

