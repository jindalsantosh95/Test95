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
using VAA.EAPI.NDC.Offer.Application.Query.OrderReshop;
using VAA.EAPI.NDC.Offer.Application.Query.OrderReshopCancel;
using VAA.EAPI.NDC.Offer.Application.Query.OrderReshopNameCorrection;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.OrderReshopRQ;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.OrderReshopRS;

namespace VAA.EAPI.NDC.Offer.Triggers.Http
{
    public class OrderReshopController
    {
        private readonly IMediator _mediator;

        public OrderReshopController(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [OpenApiOperation(operationId: "OrderReshop", tags: ["OrderReshop"], Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [Function("OrderReshop")]
        public async Task<HttpResponseData> OrderReshop([HttpTrigger(AuthorizationLevel.Anonymous, "POST", Route = "OrderReshop")] HttpRequestData httpRQ)
        {
            var orderReshopRQ = Common.Serialisation.Xml.Deserialize<IATA_OrderReshopRQ>(await new StreamReader(httpRQ.Body).ReadToEndAsync());

            var subscriptionHeaders = Enumerable.Empty<string>();
            httpRQ.Headers?.TryGetValues(Constants.Application.ApimSubscription, out subscriptionHeaders);
            var subscriptionKey = subscriptionHeaders.FirstOrDefault();

            bool isCancelOrder = ((orderReshopRQ.Request.UpdateOrder?.Item) as CancelOrderRefType) != null;
            
            var reshopOrder = ((orderReshopRQ.Request.UpdateOrder?.Item) as ReshopOrderType);
            bool isNameCorrectionOrder = reshopOrder?.ReshopOrderChoice?.Item as UpdatePaxNameType != null;
           

            var result = isCancelOrder ? await _mediator.Send(new OrderReshopCancelQuery(orderReshopRQ, subscriptionKey))
               : isNameCorrectionOrder ? await _mediator.Send(new OrderReshopNameCorrectionQuery(orderReshopRQ, subscriptionKey))
               : await _mediator.Send(new OrderReshopQuery(orderReshopRQ, subscriptionKey));

            return await ResultExtention<IATA_OrderReshopRS>.ExecuteResultAsync(httpRQ, result, true);
        }
    }
}
