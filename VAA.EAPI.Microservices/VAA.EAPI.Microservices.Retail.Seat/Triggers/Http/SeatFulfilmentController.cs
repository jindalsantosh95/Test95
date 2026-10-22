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
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Seat.Application.Command;
using VAA.EAPI.Microservices.Retail.Seat.Domain.Enums;
using VAA.EAPI.Microservices.Retail.Seat.Domain.SeatAvailability;
using VAA.EAPI.Microservices.Retail.Seat.Application.Query.SeatAvailability;
using VAA.EAPI.Microservices.Retail.Seat.Domain.Seats;
using VAA.EAPI.Microservices.Retail.Seat.Application;

namespace VAA.EAPI.Microservices.Retail.Seat.Triggers.Http
{
    public class SeatFulfilmentController
    {
        private readonly IMediator _mediator;
        private readonly ISeatService _seatService;

        public SeatFulfilmentController(IMediator mediator, ISeatService seatService)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _seatService = seatService ?? throw new ArgumentNullException(nameof(seatService));
        }


        [OpenApiOperation(operationId: "SeatFulfilment", tags: new[] { "Seat fulfilment" }, Summary = "Purchase the seat for VS Segments", Description = "Purchase free or paid seat for all VS Segments", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody("application/json", typeof(SeatFulfilmentRequest), Description = "")]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(SeatFulfilmentResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("SeatFulfilment")]
        public async Task<HttpResponseData> SeatFulfilment([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "SeatFulfilment")] HttpRequestData req)
        {
            var seatFulfilmentCommand = Json.Deserialize<SeatFulfilmentCommand>(await new StreamReader(req.Body).ReadToEndAsync());
            seatFulfilmentCommand.ChannelId = req.Headers.Contains(Constants.Application.ChannelId) ? req?.Headers.GetValues(Constants.Application.ChannelId).First() : null;

            var commandOutput = await _mediator.Send(seatFulfilmentCommand);

            return await ResultExtention<string>.ExecuteResultAsync(req, commandOutput);
        }



        [OpenApiOperation(operationId: "SeatFulfilmentResponse", tags: new[] { "SeatFulfilment Microservice" }, Summary = "Get Seat Add event response", Description = "It returns seat add event response and status against requested corelationId", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "correlationid", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Correlation id")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(SeatFulfilmentResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("SeatFulfilmentResponse")]
        public async Task<HttpResponseData> SeatFulfilmentResponse([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "SeatFulfilment/{correlationid}")] HttpRequestData req, string correlationid)
        {
            var commandOutput = await _mediator.Send(new SeatFulfilmentResponseQuery { CorrelationId = correlationid, Type = EnumOperations.GetEnumDescription((SeatFulfilmentEventType)Enum.Parse(typeof(SeatFulfilmentEventType), SeatFulfilmentEventType.SeatFulfilmentInitiate.ToString())) });

            return await ResultExtention<SeatFulfilmentResponse>.ExecuteResultAsync(req, commandOutput);
        }

        [OpenApiOperation(operationId: "SeatDeletion", tags: new[] { "Seat Microservice" }, Summary = "Deletes seats from the booking", Description = "http endpoint to Delete seats", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]        
        [OpenApiRequestBody("application/json", typeof(SeatDeletionRequest), Description = "")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Accepted, CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "Request accepted", Description = "Request accepted")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("SeatDeletion")]
        public async Task<HttpResponseData> SeatDeletion([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "seat/delete")] HttpRequestData req, string orderid)
        {
            var request = await new StreamReader(req.Body).ReadToEndAsync();
            var seatDeletionRequest = Json.Deserialize<SeatDeletionRequest>(request);
            seatDeletionRequest.ChannelId = seatDeletionRequest.ChannelId ?? (req.Headers.Contains(Constants.Application.ChannelId) ? req?.Headers.GetValues(Constants.Application.ChannelId).First() : null);
            
            var response = await _seatService.DeleteSeatAsync(seatDeletionRequest);

            return await ResultExtention<string>.ExecuteResultAsync(req, response);
        }

        [OpenApiOperation(operationId: "SeatDeletionResponse", tags: new[] { "Seat Microservice" }, Summary = "Get seat deletion event response", Description = "It returns seat deletion event response and status against requested corelationId", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "correlationid", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Correlation id")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(SeatDeletionResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("SeatDeletionResponse")]
        public async Task<HttpResponseData> SeatDeletionResponse([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "seat/delete/{correlationid}")] HttpRequestData req, string correlationid)
        {            
            var response = await _seatService.GetDeleteSeatResponseAsync(correlationid, EnumOperations.GetEnumDescription((SeatFulfilmentEventType)Enum.Parse(typeof(SeatFulfilmentEventType), SeatFulfilmentEventType.SeatDeletionInitiate.ToString())));
            return await ResultExtention<SeatDeletionResponse>.ExecuteResultAsync(req, response);
        }

    }
}
