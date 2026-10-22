using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Configuration.Domain;
using VAA.EAPI.Microservices.Configuration.Infrastructure.Interface;

namespace VAA.EAPI.Microservices.Configuration.Application
{
    public class PccCurrencyQueryHandler : IRequestHandler<PccCurrencyQuery, PccCurrencyData>
    {
        private readonly IPccCurrencyDetails _pccCurrencyDetails;

        public PccCurrencyQueryHandler(IPccCurrencyDetails PccCurrencyDetails)
        {
            _pccCurrencyDetails = PccCurrencyDetails ?? throw new ArgumentNullException(nameof(_pccCurrencyDetails));
        }

        public async Task<PccCurrencyData> Handle(PccCurrencyQuery request, CancellationToken cancellationToken)
        {
            var pccCurrencies = await _pccCurrencyDetails.GetPccCurrencyList();
            var result = (PccCurrencyData)pccCurrencies.FirstOrDefault(x => x.CurrencyCode == request.CurrencyCode);

            return result;
        }
    }
}
