using CSharpFunctionalExtensions;
using MediatR;
using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Sync.Domain.Entities.PointsCurrencyConverter;

namespace VAA.EAPI.Microservices.Loyalty.Sync.Infrastructure.Services.CurrencyConverter
{
    public class CurrencyConverterServiceCommand : IRequest<Result<Response<IEnumerable<PointsCurrencyConverterResponse>>>>
    {
        public IEnumerable<PointsCurrencyConverterRequest> CurrencyConverterRequest { get; set; }
    }
}