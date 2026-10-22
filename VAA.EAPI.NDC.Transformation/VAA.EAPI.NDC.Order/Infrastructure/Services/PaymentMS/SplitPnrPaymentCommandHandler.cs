using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.NDC.Order.Infrastructure.Model.PaymentMS;

namespace VAA.EAPI.NDC.Order.Infrastructure.Services.PaymentMS
{
    public class SplitPnrPaymentCommandHandler(IHttpConnector httpConnector) : ISplitPnrPaymentCommand
    {
        private readonly IHttpConnector _httpConnector = httpConnector;

        public async Task<SplitPaymentRS> Handle(SplitPnrPaymentRequest request)
        {
            var headers = new Dictionary<string, string>()
            {
                { "Ocp-Apim-Subscription-Key", ConfigReader.Get(Constants.Environment.PaymentMSSubscriptionKey) }
            };

            var httpResponseMessage = await _httpConnector.SendAsync(ConfigReader.Get(Constants.Environment.PaymentMSSplitPnrUrl), headers, string.Empty, HttpMethod.Post, Json.Serialize(request));

            var response = await httpResponseMessage.Content.ReadAsStringAsync();

            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
            {
                return new SplitPaymentRS() { PaymentDocument = Json.Deserialize<IEnumerable<PaymentDocument>>(response) };
            }
            else
            {
                return new SplitPaymentRS() { Errors = Json.Deserialize<List<ErrorDetails>>(response) };
            }
        }
    }
}
