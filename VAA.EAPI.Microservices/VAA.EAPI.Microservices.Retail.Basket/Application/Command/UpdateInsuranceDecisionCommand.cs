using CSharpFunctionalExtensions;
using MediatR;
using System;
using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.Microservices.Retail.Basket.Application.Command
{
    public class UpdateInsuranceDecisionCommand : IRequest<Result<Response<Domain.Basket>>>
    {
        public Guid Id { get; set; }   
       
        public bool IsInsured { get; set; } 
    }
}