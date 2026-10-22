using CSharpFunctionalExtensions;
using MediatR;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Interfaces;

namespace VAA.EAPI.NDC.Order.Infrastructure.Services.OrderDataMS
{
    public class OrderDataMSCommandHandler(IHttpConnector httpConnector) : IRequestHandler<OrderDataMSCommand, Result<Unit>>
    {
        private readonly IHttpConnector _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));

        public async Task<Result<Unit>> Handle(OrderDataMSCommand request, CancellationToken cancellationToken)
        {
            var headers = new Dictionary<string, string>()
            {
                { "Ocp-Apim-Subscription-Key", ConfigReader.Get(Constants.Environment.OrderDataMSSubscriptionKey)},
            };

            await _httpConnector.SendAsync($"{ConfigReader.Get(Constants.Environment.OrderDataMSUpdateEndpoint)}", headers, string.Empty, HttpMethod.Patch, Json.Serialize(request.OrderDocument));

            return Unit.Value;
        }
    }
}
