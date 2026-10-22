using MediatR;
using System.Collections.Generic;
using VAA.EAPI.Microservices.Configuration.Domain;

namespace VAA.EAPI.Microservices.Configuration.Application
{
    public class BspMarketCurrencyQuery : IRequest<BspMarketCurrencyData>
    {
        public string CountryCode { get; set; }
    }
}
