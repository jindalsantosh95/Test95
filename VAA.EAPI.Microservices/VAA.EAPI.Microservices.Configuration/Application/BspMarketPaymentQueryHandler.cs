using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Configuration.Domain;
using VAA.EAPI.Microservices.Configuration.Infrastructure.Interface;

namespace VAA.EAPI.Microservices.Configuration.Application
{
    public class BspMarketPaymentQueryHandler : IRequestHandler<BspMarketPaymentQuery, BspMarketPaymentData>
    {
        private readonly IBspMarketPaymentDetails _bspMarketPaymentDetails;

        public BspMarketPaymentQueryHandler(IBspMarketPaymentDetails BspMarketPaymentDetails)
        {
            _bspMarketPaymentDetails = BspMarketPaymentDetails ?? throw new ArgumentNullException(nameof(_bspMarketPaymentDetails));
        }

        public async Task<BspMarketPaymentData> Handle(BspMarketPaymentQuery request, CancellationToken cancellationToken)
        {
            var bspMarketPayments = await _bspMarketPaymentDetails.GetBspMarketPaymentList();
            var result = (BspMarketPaymentData)bspMarketPayments.FirstOrDefault(x => x.CountryCode == request.CountryCode);

            return result;
        }
    }
}
