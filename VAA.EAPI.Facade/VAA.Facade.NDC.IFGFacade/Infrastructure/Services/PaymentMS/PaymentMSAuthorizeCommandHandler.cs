using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.Facade.NDC.IFGFacade.Domain;
using VAA.Facade.NDC.IFGFacade.Infrastructure.Models.PaymentMS;

namespace VAA.Facade.NDC.IFGFacade.Infrastructure.Services.PaymentMS
{
    public class PaymentMSAuthorizeCommandHandler(IHttpConnector httpConnector) : IPaymentMSAuthorizeCommand
    {
        private readonly IHttpConnector _httpConnector = httpConnector;

        public async Task<PaymentMSResponse> Handle(AuthorizePaymentRequest request)
        {
            var headers = new Dictionary<string, string>()
            {
                { "Ocp-Apim-Subscription-Key", ConfigReader.Get(Constants.Environment.PaymentMSSubscriptionKey) }
            };

            var httpResponseMessage = await _httpConnector.SendAsync($"{ConfigReader.Get(Constants.Environment.PaymentMSUrl)}{Constants.Application.AuthorizePayment}", headers, string.Empty, HttpMethod.Post, Json.Serialize(request));

            var response = await httpResponseMessage.Content.ReadAsStringAsync();

            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
            {
                return new PaymentMSResponse() { PaymentDocument = Json.Deserialize<PaymentResponse>(response) };
            }
            else
            {
                return new PaymentMSResponse() { Errors = Json.Deserialize<List<ErrorDetails>>(response) };
            }
        }
    }
}
