using VAA.EAPI.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaMyTrips;
using VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Services.DeltaPublic.interfaces;

namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Services.DeltaPublic
{
    public class DeltaTripsDetailsService : IDeltaTripsDetailsService
    {
        private readonly IHttpConnector _apiConnector;

        public DeltaTripsDetailsService(IHttpConnector apiConnector)
        {
            _apiConnector = apiConnector ?? throw new ArgumentNullException(nameof(apiConnector));
        }

        public async Task<DeltaTripsDetailsResponse> RetrieveTripsDetails(DeltaTripsDetailsRequest deltaTripsDetailsRequest)
        {
            var dlMyTripUri = ConfigReader.Get(Constant.Environment.DlMyTripsUri);
            var headers = new Dictionary<string, string>
                {
                    {Constant.Application.TransactionId,  AsyncContext.Current.CorrelationId.ToString()},
                    {Constant.Application.ChannelId, Constant.Application.DFMobileChannelId},
                    {Constant.Application.AppId,Constant.Application.DFAppId},
                    {Constant.Environment.x_air4_mytrips_test ,ConfigReader.Try(Constant.Environment.x_air4_mytrips_test)}
                };

            return await _apiConnector.SendAsync<DeltaTripsDetailsResponse>(dlMyTripUri, headers: headers, bearerToken: null, verb: HttpMethod.Post, content: Json.Serialize(deltaTripsDetailsRequest));
        }
    }
}
