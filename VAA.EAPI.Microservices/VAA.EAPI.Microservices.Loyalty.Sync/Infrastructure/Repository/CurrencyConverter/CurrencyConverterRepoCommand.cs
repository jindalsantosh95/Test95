using CSharpFunctionalExtensions;
using MediatR;
using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Sync.Domain.Entities.PointsCurrencyConverter;

namespace VAA.EAPI.Microservices.Loyalty.Sync.Infrastructure.Repository.CurrencyConverter
{
    public class CurrencyConverterRepoCommand : IRequest<Result<Response<IEnumerable<PointsCurrencyConverterResponse>>>>
    {
        public IEnumerable<PointsCurrencyConverterRequest> Request { get; set; }
    }
}