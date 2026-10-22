using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
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
using VAA.EAPI.Microservices.Retail.Order.Notification.Application.Commands;
using VAA.EAPI.Microservices.Retail.Order.Notification.Domain.Entities;

namespace VAA.EAPI.Microservices.Retail.Order.Notification.Triggers.Http
{
    public class OrderNotificationController
    {
        private readonly IMediator _mediator;
        private readonly ILogger<OrderNotificationController> _logger;
        public OrderNotificationController(IMediator mediator, ILogger<OrderNotificationController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [OpenApiOperation(operationId: "OrderNotification", tags: new[] { "Order Notification Microservice" }, Summary = "Provides ability to add unique reference of an order in VAA system", Description = "This API provides ability to add an order reference  of a customer booking in VAA system. It's currently handling limited Channel e.g. Website-VAA booking(New Digital flow).", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiParameter(name: "channel", In = ParameterLocation.Header, Required = true, Type = typeof(string), Description = "ID for Channel used to generate order, like Web-DF,Web-Vhols,Ndc,App-DF,CallCentre-Vhols", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody("text/plain", typeof(OrderNotificationRequest), Description = "")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Accepted, Summary = "Accepted", Description = "Accepted")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "BadRequest", Description = "BadRequest")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]

        [Function("OrderNotification")]
        public async Task<HttpResponseData> OrderNotification([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "order/notification")] HttpRequestData req)
        {
            var request = await new StreamReader(req.Body).ReadToEndAsync();
            OrderNotificationRequest orderNotifyPayload = Json.Deserialize<OrderNotificationRequest>(request);
            string channel = req.Headers.Any(k => string.Equals(k.Key, Constants.Application.ChannelPathParam)) ? req.Headers.GetValues(Constants.Application.ChannelPathParam).First() : null;
            _logger.LogDebug($"OrderNotification Controller | Channel : {channel}");

            var commandOutput = await _mediator.Send
                (
                    request: new OrderNotificationRequestCommand()
                    {
                        NotificationRequest = orderNotifyPayload,
                        Channel = channel
                    },
                    cancellationToken: CancellationToken.None
                );
            return await ResultExtention<string>.ExecuteResultAsync(req, commandOutput);
        }


        //[OpenApiOperation(operationId: "OrderNotificationStatus", tags: new[] { "Order Notification Microservice" }, Summary = "Provides ability to poll order status of an order from VAA system", Description = "Performs a order status check", Visibility = OpenApiVisibilityType.Important)]
        //[OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        //[OpenApiParameter(name: "correlationid", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Correlation Id")]
        //[OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(OrderNotificationStatusResponse), Summary = "OK", Description = "OK")]
        //[OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        //[OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]

        /////<summary>
        /////OrderNotificationStatus is not in use as per user story -  296010
        /////</summary>

        ////[Function("OrderNotificationStatus")] //Commented as part of user story 296010
        //public async Task<HttpResponseData> OrderNotificationStatus([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "order/notification/Status/{correlationid}")] HttpRequestData req, string correlationid)
        //{
        //    _logger.LogInformation($"OrderNotificationStatus Controller | CorrelationId: {correlationid}");

        //    var commandOutput = await _mediator.Send
        //        (
        //            request: new OrderNotificationStatusQuery
        //            {
        //                CorrelationId = correlationid,
        //                Type = Constants.Application.OrderNotificationEventSubject
        //            },
        //            cancellationToken: CancellationToken.None
        //        );

        //    return await ResultExtention<OrderNotificationStatusResponse>.ExecuteResultAsync
        //        (
        //            request: req,
        //            result: commandOutput
        //        );
        //}

    }
}
