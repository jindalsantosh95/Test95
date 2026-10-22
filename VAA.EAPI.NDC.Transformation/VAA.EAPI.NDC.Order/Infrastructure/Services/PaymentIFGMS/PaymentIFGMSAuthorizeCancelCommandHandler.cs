using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.NDC.Order.Infrastructure.Interface;
using VAA.EAPI.NDC.Order.Infrastructure.Model.PaymentMS;

namespace VAA.EAPI.NDC.Order.Infrastructure.Services.PaymentIFGMS
{
    public class PaymentIFGMSAuthorizeCancelCommandHandler : IPaymentIFGMSAuthorizeCancelCommand
    {
        private readonly IHttpConnector _httpConnector;

        public PaymentIFGMSAuthorizeCancelCommandHandler(IHttpConnector httpConnector)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        }

        public async Task Handle(AuthorizePaymentRequest request, CancellationToken cancellationToken)
        {
            await HandleWithStatusAsync(request, CancellationToken.None);
        }

        public async Task<bool> HandleWithStatusAsync(AuthorizePaymentRequest request, CancellationToken cancellationToken)
        {
            var headers = new Dictionary<string, string>()
            {
                { "Ocp-Apim-Subscription-Key", ConfigReader.Get(Constants.Environment.PaymentIFGMSSubscriptionKey) }

            };

            using var content = new StringContent(Json.Serialize(request), Encoding.UTF8, "application/json");
            using var httpResponseMessage = await _httpConnector.SendAsync(ConfigReader.Get(Constants.Environment.PaymentIFGMSAuthorizeCancelUrl), headers, string.Empty, HttpMethod.Post, content, cancellationToken);
            return httpResponseMessage.IsSuccessStatusCode;
        }
    }
}
