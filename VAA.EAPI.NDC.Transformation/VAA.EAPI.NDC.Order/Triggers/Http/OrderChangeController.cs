using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.NDC.Order.Application.OrderChange;
using VAA.EAPI.NDC.Order.Constants;
using VAA.EAPI.NDC.Order.Domain;
using VAA.EAPI.NDC.Order.Domain.OrderChangeRQ;
using VAA.EAPI.NDC.Order.Domain.OrderRetrieveRQ;
using VAA.EAPI.NDC.Order.Domain.OrderViewRS;

namespace VAA.EAPI.NDC.Order.Triggers.Http
{
    public class OrderChangeController
    {
        private readonly IMediator _mediator;
        private readonly IOrderChangeAmendApisCommand _orderChangeAmendApisCommand;
        private readonly IOrderChangeSplitPnrCommand _orderChangeSplitPnrCommand;
        private readonly IOrderChangeDisruptionCommand _orderChangeDisruptionCommand;
        private readonly IOrderChangeCancelHoldBookingCommand _orderChangeCancelHoldBookingCommand;

        public OrderChangeController(IMediator mediator, IOrderChangeAmendApisCommand orderChangeAmendApisCommand, IOrderChangeSplitPnrCommand orderChangeSplitPnrCommand, IOrderChangeDisruptionCommand orderChangeDisruptionCommand, IOrderChangeCancelHoldBookingCommand orderChangeCancelHoldBookingCommand)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _orderChangeAmendApisCommand = orderChangeAmendApisCommand ?? throw new ArgumentNullException(nameof(orderChangeAmendApisCommand));
            _orderChangeSplitPnrCommand = orderChangeSplitPnrCommand ?? throw new ArgumentNullException(nameof(orderChangeSplitPnrCommand));
            _orderChangeDisruptionCommand = orderChangeDisruptionCommand ?? throw new ArgumentNullException(nameof(orderChangeDisruptionCommand));
            _orderChangeCancelHoldBookingCommand = orderChangeCancelHoldBookingCommand ?? throw new ArgumentNullException(nameof(orderChangeCancelHoldBookingCommand));
        }

        [OpenApiOperation(operationId: "OrderChange", tags: new[] { "Order Change" }, Summary = "", Description = "", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody("application/xml", typeof(IATA_OrderRetrieveRQ), Description = "")]
        [OpenApiParameter(name: "channel", In = ParameterLocation.Header, Required = true, Type = typeof(string), Description = "Channel id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Xml, bodyType: typeof(IATA_OrderViewRS), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/xml", bodyType: typeof(List<IATA_OrderViewRS>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/xml", bodyType: typeof(List<IATA_OrderViewRS>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("OrderChange")]
        public async Task<HttpResponseData> OrderChange([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "OrderChange")] HttpRequestData req)
        {
            var orderChangeReq = Xml.Deserialize<IATA_OrderChangeRQ>(await new StreamReader(req.Body).ReadToEndAsync());
            
            var augElement = orderChangeReq?.AugmentationPoint?.Any?.ElementAtOrDefault(0) as XmlElement;
            var augmentationPoint = new AugmentationPoint();

            if (augElement != null)
            {
                augmentationPoint.PaxRefID = augElement.GetElementsByTagName("PaxRefID").Cast<XmlElement>().Select(x => x.InnerText.Trim()).Where(x => !string.IsNullOrEmpty(x));
            }
            
            var requestType = VAA.EAPI.NDC.Order.Helper.OrderChangeRequestType.IdentifyRequestType(orderChangeReq, augmentationPoint);

            string subscriptionKey = req.Headers?.GetValues("Ocp-Apim-Subscription-Key").FirstOrDefault();

            var commandOutput = requestType switch
            {
                OrderChangeType.HoldBooking => await _mediator.Send(new OrderChangeHoldBookingCommand { OrderChangeRQ = orderChangeReq, SubscriptionKey = subscriptionKey }, CancellationToken.None),
                OrderChangeType.PostSaleSeat => await _mediator.Send(new PostSaleOrderChangeCommand { OrderChangeRQ = orderChangeReq, SubscriptionKey = subscriptionKey }, CancellationToken.None),
                OrderChangeType.AddCollect => await _mediator.Send(new OrderChangeAddColCommand { OrderChangeRQ = orderChangeReq, SubscriptionKey = subscriptionKey }, CancellationToken.None),
                OrderChangeType.Cancel => await _mediator.Send(new OrderChangeCancelCommand { OrderChangeRQ = orderChangeReq, SubscriptionKey = subscriptionKey }, CancellationToken.None),
                OrderChangeType.EvenExchange => await _mediator.Send(new OrderChangeEvenExchangeCommand { OrderChangeRQ = orderChangeReq, SubscriptionKey = subscriptionKey }, CancellationToken.None),
                OrderChangeType.Refund => await _mediator.Send(new OrderChangeRefundCommand { OrderChangeRQ = orderChangeReq, SubscriptionKey = subscriptionKey }, CancellationToken.None),
                OrderChangeType.AmendApis => await _orderChangeAmendApisCommand.Handle(orderChangeReq, subscriptionKey, CancellationToken.None),
                OrderChangeType.SplitPnr => await _orderChangeSplitPnrCommand.Handle(orderChangeReq, augmentationPoint,subscriptionKey, CancellationToken.None),
                OrderChangeType.Disruption => await _orderChangeDisruptionCommand.Handle(orderChangeReq, subscriptionKey ,CancellationToken.None),
                OrderChangeType.CancelHoldBooking => await _orderChangeCancelHoldBookingCommand.Handle(orderChangeReq, subscriptionKey, CancellationToken.None),

            };

            return await ResultExtention<IATA_OrderViewRS>.ExecuteResultAsync(req, commandOutput, true);
        }
    }
}
