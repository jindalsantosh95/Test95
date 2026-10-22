using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Seat.Infrastructure.Proxy.SeatV8Proxy;

namespace VAA.EAPI.Microservices.Retail.Seat.Infrastructure.Services.SeatAvailability
{
    public class Air4SeatV8 : IAir4SeatV8
    {
        private readonly SeatClient _seatClient;
        private readonly ILogger<Air4SeatV8> _logger;

        public Air4SeatV8(SeatClient seatClient, ILogger<Air4SeatV8> logger)
        {
            _seatClient = seatClient;
            _logger = logger;
        }

        public async Task<RetrievePassengerMapResponseType> RetrievePassengerMap(RetrievePassengerMapRequestType retrievePassengerMapRequestType)
        {
            _seatClient.ClientCredentials.ClientCertificate.Certificate ??= CertificateHelper.GetCachedCertificate();

            return (await _seatClient.RetrievePassengerMapAsync(retrievePassengerMapRequestType)).RetrievePassengerMapResponse1;
        }

        public async Task<AddResponseType> AddSeat(AddRequestType addRequestType)
        {
            try
            {
                _seatClient.ClientCredentials.ClientCertificate.Certificate ??= CertificateHelper.GetCachedCertificate();

                return (await _seatClient.AddAsync(addRequestType)).AddResponse1;
            }
            catch (System.ServiceModel.FaultException exception)
            {
                _logger.LogError(exception, $"Correlation id - {AsyncContext.Current.CorrelationId} DL Response: {exception.Data["Response"]?.ToString()}");
                throw;
            }

        }

        public async Task<DeleteResponseType> DeleteSeat(DeleteRequestType delRequestType)
        {
            _seatClient.ClientCredentials.ClientCertificate.Certificate ??= CertificateHelper.GetCachedCertificate();

            return (await _seatClient.DeleteAsync(delRequestType)).DeleteResponse1;
        }
    }
}
