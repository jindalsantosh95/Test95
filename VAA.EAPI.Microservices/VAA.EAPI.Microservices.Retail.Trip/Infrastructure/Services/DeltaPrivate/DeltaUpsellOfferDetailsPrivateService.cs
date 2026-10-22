using System.Text;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaUpsellOffers;
using VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Services.DeltaPrivate.interfaces;

namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Services.DeltaPrivate
{
    public class DeltaUpsellOfferDetailsPrivateService : IDeltaUpsellOfferDetailsPrivateService
    {
        private readonly IHttpConnector _apiConnector;
        private readonly IAir4TokenGenerator _air4TokenGenerator;
        private static readonly Dictionary<string, string> _headers = new()
        {
                    {Constant.Application.DlUpsellAPITransactionId,  AsyncContext.Current.CorrelationId.ToString()},
                    {Constant.Application.ChannelId, Constant.Application.DFMobileChannelId},
                    {Constant.Application.AppId,Constant.Application.DFAppId},
                    {Constant.Environment.x_air4_mytrips_test ,ConfigReader.Try(Constant.Environment.x_air4_mytrips_test)}
            };

        public DeltaUpsellOfferDetailsPrivateService(IHttpConnector apiConnector, IAir4TokenGenerator air4TokenGenerator)
        {
            _apiConnector = apiConnector ?? throw new ArgumentNullException(nameof(apiConnector));
            _air4TokenGenerator = air4TokenGenerator ?? throw new ArgumentNullException(nameof(air4TokenGenerator));
        }

        public async Task<UpsellOfferResponse> RetrieveUpsellOfferDetails(string tripReservationId)
        {
            var token = await _air4TokenGenerator.GetToken(ConfigReader.Get(Constant.Application.DeltaClientIdValue), ConfigReader.Get(Constant.Application.DeltaClientSecret));

            var dlUpsellOffersUri = $"{ConfigReader.Get(Constant.Environment.DlUpsellOfffersPrivateUri).TrimEnd('/')}/{tripReservationId}";

            return await _apiConnector.SendAsync<UpsellOfferResponse>(dlUpsellOffersUri, headers: _headers, bearerToken: token, verb: HttpMethod.Get, content: new StringContent("", Encoding.UTF8, "application/json"));
        }
    }
}
