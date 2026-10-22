using Microsoft.Extensions.Logging;
using System.Text;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaUpsellOffers;
using VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Services.DeltaPublic.interfaces;

namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Services.DeltaPublic
{

    public class DeltaUpsellOfferDetailsService : IDeltaUpsellOfferDetailsService
    {
        private readonly IHttpConnector _apiConnector;
        private static readonly Dictionary<string, string> _headers = new()
        {
                    {Constant.Application.DlUpsellAPITransactionId,  AsyncContext.Current.CorrelationId.ToString()},
                    {Constant.Application.ChannelId, Constant.Application.DFMobileChannelId},
                    {Constant.Application.AppId,Constant.Application.DFAppId},
                    {Constant.Environment.x_air4_mytrips_test ,ConfigReader.Try(Constant.Environment.x_air4_mytrips_test)}
            };

        public DeltaUpsellOfferDetailsService(IHttpConnector apiConnector, ILogger<DeltaUpsellOfferDetailsService> logger)
        {
            _apiConnector = apiConnector ?? throw new ArgumentNullException(nameof(apiConnector));
        }

        public async Task<UpsellOfferResponse> RetrieveUpsellOfferDetails(string tripReservationId)
        {
            var dlUpsellOffersUri = $"{ConfigReader.Get(Constant.Environment.DlUpsellOfffersUri).TrimEnd('/')}/{tripReservationId}";

            return await _apiConnector.SendAsync<UpsellOfferResponse>(dlUpsellOffersUri, headers: _headers, bearerToken: null, verb: HttpMethod.Get, content: new StringContent("", Encoding.UTF8, "application/json"));
        }
    }
}
