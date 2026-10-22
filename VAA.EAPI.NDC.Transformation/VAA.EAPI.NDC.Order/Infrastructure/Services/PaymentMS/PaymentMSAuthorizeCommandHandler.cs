using MediatR;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.NDC.Order.Infrastructure.Model.PaymentMS;


namespace VAA.EAPI.NDC.Order.Infrastructure.Services.PaymentMS
{
    public class PaymentMSAuthorizeCommandHandler : IRequestHandler<PaymentMSAuthorizeCommand, PaymentRS>
    {
        private readonly IHttpConnector _httpConnector;

        public PaymentMSAuthorizeCommandHandler(IHttpConnector httpConnector)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        }

        public async Task<PaymentRS> Handle(PaymentMSAuthorizeCommand request, CancellationToken cancellationToken)
        {
            var headers = new Dictionary<string, string>()
            {
                { "Ocp-Apim-Subscription-Key", ConfigReader.Get(Constants.Environment.PaymentMSSubscriptionKey) }
            };            

            var httpResponseMessage = await _httpConnector.SendAsync(ConfigReader.Get(Constants.Environment.PaymentMSAuthorizeUrl), headers, string.Empty, HttpMethod.Post, Json.Serialize(request));

            var response = await httpResponseMessage.Content.ReadAsStringAsync();

            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
            {
                return new PaymentRS() { PaymentDocument = Json.Deserialize<PaymentDocument>(response) };
            }

            else
            {
                return new PaymentRS() { Errors = Json.Deserialize<List<ErrorDetails>>(response) } ;
            }           

        }
    }
}
