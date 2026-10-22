using System.Text;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaEncryptedBookingDetails;
using VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Services.DeltaPublic.interfaces;

namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Services.DeltaPublic
{
    public class DeltaEncryptionService : IDeltaEncryptionService
    {
        private readonly IHttpConnector _apiConnector;

        public DeltaEncryptionService(IHttpConnector apiConnector)
        {
            _apiConnector = apiConnector ?? throw new ArgumentNullException(nameof(apiConnector));
        }
        public async Task<EncryptionResponse> GetEncryptedBookingDetailsAsync(string tripBookingId)
        {
            var dlEncryptionServiceUri = $"{ConfigReader.Get(Constant.Environment.DlEncryptionServiceUri)}/{tripBookingId}";
            var headers = new Dictionary<string, string>
                {
                    {Constant.Application.TransactionId,  AsyncContext.Current.CorrelationId.ToString()},
                    {Constant.Application.ChannelId, Constant.Application.DFMobileChannelId},
                    {Constant.Application.AppId,Constant.Application.DFAppId},
                {Constant.Environment.x_air4_mytrips_test ,ConfigReader.Try(Constant.Environment.x_air4_mytrips_test)}
            };

            return await _apiConnector.SendAsync<EncryptionResponse>(dlEncryptionServiceUri, headers: headers, bearerToken: null, verb: HttpMethod.Post, content: new StringContent("", Encoding.UTF8, "application/json"));
        }
    }
}