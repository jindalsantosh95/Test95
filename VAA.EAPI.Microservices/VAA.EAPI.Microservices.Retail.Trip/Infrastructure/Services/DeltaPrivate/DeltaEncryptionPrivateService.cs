using System.Text;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaEncryptedBookingDetails;
using VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Services.DeltaPrivate.interfaces;

namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Services.DeltaPrivate
{
    public class DeltaEncryptionPrivateService : IDeltaEncryptionPrivateService
    {
        private readonly IHttpConnector _apiConnector;
        private readonly IAir4TokenGenerator _air4TokenGenerator;

        public DeltaEncryptionPrivateService(IHttpConnector apiConnector, IAir4TokenGenerator air4TokenGenerator)
        {
            _apiConnector = apiConnector ?? throw new ArgumentNullException(nameof(apiConnector));
            _air4TokenGenerator = air4TokenGenerator ?? throw new ArgumentNullException(nameof(air4TokenGenerator));
        }

        public async Task<EncryptionResponse> GetEncryptedBookingDetailsAsync(string tripBookingId)
        {
            var token = await _air4TokenGenerator.GetToken(ConfigReader.Get(Constant.Application.DeltaClientIdValue), ConfigReader.Get(Constant.Application.DeltaClientSecret));
            var dlEncryptionServicePrivateUri = $"{ConfigReader.Get(Constant.Environment.DlEncryptionServicePrivateUri)}/{tripBookingId}";
            var headers = new Dictionary<string, string>
                {
                    {Constant.Application.TransactionId,  AsyncContext.Current.CorrelationId.ToString()},
                    {Constant.Application.ChannelId, Constant.Application.DFMobileChannelId},
                    {Constant.Application.AppId,Constant.Application.DFAppId},
                {Constant.Environment.x_air4_mytrips_test ,ConfigReader.Try(Constant.Environment.x_air4_mytrips_test)}
            };

            return await _apiConnector.SendAsync<EncryptionResponse>(dlEncryptionServicePrivateUri, headers: headers, bearerToken: token, verb: HttpMethod.Post, content: new StringContent("", Encoding.UTF8, "application/json"));
        }
    }
}