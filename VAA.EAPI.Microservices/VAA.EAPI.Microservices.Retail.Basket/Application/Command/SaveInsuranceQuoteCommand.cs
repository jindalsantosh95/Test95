using CSharpFunctionalExtensions;
using MediatR;
using System;
using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Basket.Domain.Insurance;

namespace VAA.EAPI.Microservices.Retail.Basket.Application.Command
{
    public class SaveInsuranceQuoteCommand : IRequest<Result<Response<Domain.Basket>>>
    {
        public Guid Id { get; set; }   
       
        public IEnumerable<InsuranceQuote> InsuranceQuotes { get; set; }
    }
}