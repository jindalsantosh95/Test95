using VAA.EAPI.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaMyTrips;
using VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Services.DeltaPrivate.interfaces;

namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Services.DeltaPrivate
{
    public class DeltaTripsDetailsPrivateService : IDeltaTripsDetailsPrivateService
    {
        private readonly IHttpConnector _apiConnector;
        private readonly IAir4TokenGenerator _air4TokenGenerator;

        public DeltaTripsDetailsPrivateService(IHttpConnector apiConnector, IAir4TokenGenerator air4TokenGenerator)
        {
            _apiConnector = apiConnector ?? throw new ArgumentNullException(nameof(apiConnector));
            _air4TokenGenerator = air4TokenGenerator ?? throw new ArgumentNullException(nameof(air4TokenGenerator));
        }

        public async Task<DeltaTripsDetailsResponse> RetrieveTripsDetails(DeltaTripsDetailsRequest deltaTripsDetailsRequest)
        {

            var token = await _air4TokenGenerator.GetToken(ConfigReader.Get(Constant.Application.DeltaClientIdValue), ConfigReader.Get(Constant.Application.DeltaClientSecret));

            var dlMyTripUri = ConfigReader.Get(Constant.Environment.DlMyTripsPrivateUri);
            var headers = new Dictionary<string, string>
                {
                    {Constant.Application.TransactionId,  AsyncContext.Current.CorrelationId.ToString()},
                    {Constant.Application.ChannelId, Constant.Application.DFMobileChannelId},
                    {Constant.Application.AppId,Constant.Application.DFAppId},
                    {Constant.Environment.x_air4_mytrips_test ,ConfigReader.Try(Constant.Environment.x_air4_mytrips_test)}
                };

            return await _apiConnector.SendAsync<DeltaTripsDetailsResponse>(dlMyTripUri, headers: headers, bearerToken: token, verb: HttpMethod.Post, content: Json.Serialize(deltaTripsDetailsRequest));
        }
    }
}
