using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.NDC.Offer.Application.Query.OrderQuote;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.OrderQuoteRQ;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.OrderReshopRS;

namespace VAA.EAPI.NDC.Offer.Triggers.Http
{
    public class OrderQuoteController
    {
        private readonly IMediator _mediator;

        public OrderQuoteController(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [OpenApiOperation(operationId: "OrderQuote", tags: ["OrderQuote"], Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [Function("OrderQuote")]
        public async Task<HttpResponseData> OrderQuote([HttpTrigger(AuthorizationLevel.Anonymous, "POST", Route = "OrderQuote")] HttpRequestData httpRQ)
        {
            var orderQuoteRQ = EAPI.Common.Serialisation.Xml.Deserialize<IATA_OrderQuoteRQ>(await new StreamReader(httpRQ.Body).ReadToEndAsync());

            var subscriptionHeaders = Enumerable.Empty<string>();
            httpRQ.Headers?.TryGetValues(Constants.Application.ApimSubscription, out subscriptionHeaders);
            var subscriptionKey = subscriptionHeaders.FirstOrDefault();

            var result = await _mediator.Send(new OrderQuoteQuery(orderQuoteRQ, subscriptionKey));

            return await ResultExtention<IATA_OrderReshopRS>.ExecuteResultAsync(httpRQ, result, true);
        }
    }
}
