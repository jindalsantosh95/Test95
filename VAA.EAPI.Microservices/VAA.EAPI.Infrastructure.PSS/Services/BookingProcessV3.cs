using System;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.PSS.Proxy;
using VAA.EAPI.Infrastructure.PSS.Services.Interfaces;

namespace VAA.EAPI.Infrastructure.PSS.Services
{
    public class BookingProcessV3 : IBookingProcessV3
    {
        readonly BookingProcessClient _bookingProcessClient;

        public BookingProcessV3(BookingProcessClient bookingProcessClient)
        {
            _bookingProcessClient = bookingProcessClient ?? throw new ArgumentNullException(nameof(bookingProcessClient));
        }

        public async Task<PerformRevenueResponseType> PerformRevenue(PerformRevenueRequestType performRevenueRequest)
        {

            if (_bookingProcessClient.ClientCredentials.ClientCertificate.Certificate == null)
            {
                _bookingProcessClient.ClientCredentials.ClientCertificate.Certificate = CertificateHelper.GetCachedCertificate();
            }

            return (await _bookingProcessClient.PerformRevenueAsync(performRevenueRequest)).PerformRevenueResponse1;
        }
    }
}
