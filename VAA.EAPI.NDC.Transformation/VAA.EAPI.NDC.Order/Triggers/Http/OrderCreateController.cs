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
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.NDC.Order.Application.OrderCreate;
using VAA.EAPI.NDC.Order.Domain.OrderCreateRQ;
using VAA.EAPI.NDC.Order.Domain.OrderViewRS;

namespace VAA.EAPI.NDC.Order.Triggers.Http
{
    public class OrderCreateController
    {
        private readonly IMediator _mediator;
        private readonly IOrderCreateIFGCommand _orderCreateIFGCommand;
        private readonly IOrderCreateEasyPayCommand _orderCreateEasyPayCommand;

        public OrderCreateController(IMediator mediator, IOrderCreateIFGCommand orderCreateIFGCommand, IOrderCreateEasyPayCommand orderCreateEasyPayCommand)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _orderCreateIFGCommand = orderCreateIFGCommand ?? throw new ArgumentNullException(nameof(orderCreateIFGCommand));
            _orderCreateEasyPayCommand = orderCreateEasyPayCommand;
        }

        [OpenApiOperation(operationId: "OrderCreate", tags: new[] { "Order Create" }, Summary = "This endpoint is used to create PNR", Description = "This endpoint is used to create PNR", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody("application/xml", typeof(IATA_OrderCreateRQ), Description = "")]
        [OpenApiParameter(name: "channel", In = ParameterLocation.Header, Required = true, Type = typeof(string), Description = "Channel id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Xml, bodyType: typeof(IATA_OrderViewRS), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/xml", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/xml", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("OrderCreate")]
        public async Task<HttpResponseData> OrderCreate([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "OrderCreate")] HttpRequestData req)
        {
            var orderCreateRequest = Xml.Deserialize<IATA_OrderCreateRQ>(await new StreamReader(req.Body).ReadToEndAsync());
            string subscriptionKey = req.Headers?.GetValues("Ocp-Apim-Subscription-Key").FirstOrDefault();

            bool isCashPayment = orderCreateRequest.Request?.PaymentFunctions?.FirstOrDefault()?.PaymentProcessingDetails?.PaymentMethod?.Item is Domain.OrderCreateRQ.SettlementPlanType;
            bool isEasyPay = orderCreateRequest.Request?.PaymentFunctions?.FirstOrDefault()?.PaymentProcessingDetails?.PaymentMethod?.Item is PaymentProcessingIATA_EasyPayType;
            
            var commandOutput = isEasyPay ? await _orderCreateEasyPayCommand.Handle(orderCreateRequest, isEasyPay, subscriptionKey, CancellationToken.None)
                  : isCashPayment ? await _orderCreateIFGCommand.Handle(orderCreateRequest, subscriptionKey, CancellationToken.None)
                  : await _mediator.Send(new OrderCreateCommand() { OrderCreateRQ = orderCreateRequest, SubscriptionKey = subscriptionKey }, CancellationToken.None);

            return await ResultExtention<IATA_OrderViewRS>.ExecuteResultAsync(req, commandOutput, true);
        }
    }
}