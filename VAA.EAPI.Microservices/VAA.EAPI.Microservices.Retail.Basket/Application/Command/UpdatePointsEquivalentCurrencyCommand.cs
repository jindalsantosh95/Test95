using CSharpFunctionalExtensions;
using MediatR;
using System;
using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Basket.Domain.PointsCurrencyConverter;

namespace VAA.EAPI.Microservices.Retail.Basket.Application.Command
{
    public class UpdatePointsEquivalentCurrencyCommand: IRequest<Result<Response<Domain.Basket>>>
    {
        public Guid Id { get; set; }
        public IEnumerable<PtcWisePointsEquivalentCurrencyDetails> PtcWisePointsEquivalentCurrencyDetails { get; set; }
    }
}