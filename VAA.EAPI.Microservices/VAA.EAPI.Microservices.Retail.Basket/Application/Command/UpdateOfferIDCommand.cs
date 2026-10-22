using CSharpFunctionalExtensions;
using MediatR;
using System;
using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.Microservices.Retail.Basket.Application.Command
{
    public class UpdateOfferIDCommand : IRequest<Result<Response<Domain.Basket>>>
    {
        public Guid Id { get; set; }
        public IEnumerable<string> OfferIDs { get; set; }
    }
}
