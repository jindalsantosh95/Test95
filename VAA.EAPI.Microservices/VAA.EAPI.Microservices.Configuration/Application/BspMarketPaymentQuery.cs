using MediatR;
using VAA.EAPI.Microservices.Configuration.Domain;

namespace VAA.EAPI.Microservices.Configuration.Application
{
    public class BspMarketPaymentQuery : IRequest<BspMarketPaymentData>
    {
        public string CountryCode { get; set; }
    }
}
