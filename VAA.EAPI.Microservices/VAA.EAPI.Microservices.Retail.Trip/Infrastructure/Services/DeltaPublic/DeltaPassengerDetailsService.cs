using Microsoft.Extensions.Logging;
using System.Text;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaPassengerInfo;
using VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Services.DeltaPublic.interfaces;

namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Services.DeltaPublic
{
    public class DeltaPassengerDetailsService : IDeltaPassengerDetailsService
    {
        private readonly IHttpConnector _apiConnector;
        private static readonly Dictionary<string, string> _headers = new()
        {
                    {Constant.Application.TransactionId,  AsyncContext.Current.CorrelationId.ToString()},
                    {Constant.Application.ChannelId, Constant.Application.DFMobileChannelId},
                    {Constant.Application.AppId,Constant.Application.DFAppId},
                    {Constant.Environment.x_air4_mytrips_test ,ConfigReader.Try(Constant.Environment.x_air4_mytrips_test)}
            };

        public DeltaPassengerDetailsService(IHttpConnector apiConnector, ILogger<DeltaTripsDetailsService> logger)
        {
            _apiConnector = apiConnector ?? throw new ArgumentNullException(nameof(apiConnector));
        }

        public async Task<PassengerInformationResponse> RetrievePassengerDetails(string tripReservationId)
        {
            var dlPaxInfoUri = $"{ConfigReader.Get(Constant.Environment.DlPassengerInfoUri).TrimEnd('/')}/{tripReservationId}";

            return await _apiConnector.SendAsync<PassengerInformationResponse>(dlPaxInfoUri, headers: _headers, bearerToken: null, verb: HttpMethod.Post, content: new StringContent("", Encoding.UTF8, "application/json"));
        }

        public async Task<PassengerInformationResponse> UpdatePassengerDetails(PassengerInformation passengerInformation)
        {
            var dlPaxInfoUri = $"{ConfigReader.Get(Constant.Environment.DlPassengerInfoUri).TrimEnd('/')}/{passengerInformation.ReservationResourceId}";

            return await _apiConnector.SendAsync<PassengerInformationResponse>(dlPaxInfoUri, headers: _headers, bearerToken: null, verb: HttpMethod.Patch, content: Json.Serialize(passengerInformation));
        }
    }
}
