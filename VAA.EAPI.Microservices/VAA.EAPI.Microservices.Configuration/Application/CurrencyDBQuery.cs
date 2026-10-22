using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Configuration.Domain;

namespace VAA.EAPI.Microservices.Configuration.Application
{
    public class CurrencyDBQuery : IRequest<Result<Response<CurrencyData>>>
    {
        public string CountryCode { get; set; }
    }
}
