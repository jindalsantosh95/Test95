using CSharpFunctionalExtensions;
using MediatR;
using System;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Basket.Domain;

namespace VAA.EAPI.Microservices.Retail.Basket.Application.Command
{
    public class UpdateTnCCommand : IRequest<Result<Response<Domain.Basket>>>
    {
        public Guid Id { get; set; }
        public TermsAndConditions TermsAndConditions { get; set; }
    }
}
