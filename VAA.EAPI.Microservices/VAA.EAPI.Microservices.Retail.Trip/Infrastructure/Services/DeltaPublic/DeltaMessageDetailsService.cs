using Microsoft.Extensions.Logging;
using System.Text;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaMessageCentre;
using VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Services.DeltaPublic.interfaces;

namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Services.DeltaPublic
{
    public class DeltaMessageDetailsService : IDeltaMessageDetailsService
    {
        private readonly IHttpConnector _apiConnector;

        public DeltaMessageDetailsService(IHttpConnector apiConnector)
        {
            _apiConnector = apiConnector ?? throw new ArgumentNullException(nameof(apiConnector));
        }

        public async Task<MessageResponse> GetMessageDetailsAsync(string tripBookingID)
        {
            var dlMessageCentreUri = $"{ConfigReader.Get(Constant.Environment.DlMessageCentreUri)}{tripBookingID}";
            var headers = new Dictionary<string, string>
                {
                    {Constant.Application.TransactionId,  AsyncContext.Current.CorrelationId.ToString()},
                    {Constant.Application.ChannelId, Constant.Application.DFMobileChannelId},
                    {Constant.Application.AppId,Constant.Application.DFAppId},
                {Constant.Environment.x_air4_mytrips_test ,ConfigReader.Try(Constant.Environment.x_air4_mytrips_test)}
            };

            return await _apiConnector.SendAsync<MessageResponse>(dlMessageCentreUri, headers: headers, bearerToken: null, verb: HttpMethod.Get, content: new StringContent("", Encoding.UTF8, "application/json"));
        }
    }
}
