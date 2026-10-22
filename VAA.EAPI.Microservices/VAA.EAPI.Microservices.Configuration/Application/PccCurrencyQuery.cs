using MediatR;
using VAA.EAPI.Microservices.Configuration.Domain;

namespace VAA.EAPI.Microservices.Configuration.Application
{
    public class PccCurrencyQuery : IRequest<PccCurrencyData>
    {
        public string CurrencyCode { get; set; }
    }
}
