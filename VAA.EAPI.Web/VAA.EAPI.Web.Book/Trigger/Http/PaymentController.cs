using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using VAA.EAPI.Web.Book.Helper;
using VAA.EAPI.Web.Book.Models.Book.Confirmation;
using VAA.EAPI.Web.Book.Models.Book.Payment;
using VAA.EAPI.Web.Common;
using VAA.EAPI.Web.Common.Entities;
using VAA.EAPI.Web.Common.Extensions;
using VAA.EAPI.Web.Common.Helper;
using VAA.EAPI.Web.Common.Infrastructure.Exceptions;
using VAA.EAPI.Web.Common.Serialisation;

namespace VAA.EAPI.Web.Book.Trigger.Http
{
    public class PaymentController
    {
        private readonly IMediator _mediator;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(IMediator mediator, ILogger<PaymentController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [OpenApiOperation(operationId: "Payment", tags: new[] { "Payment" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody("application/json", typeof(PaymentRefIdRequest), Description = "Request to generate new PRI")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(PaymentResponse), Summary = "OK", Description = "OK")]
        [OpenApiParameter(name: "BasketId", In = ParameterLocation.Header, Required = true, Type = typeof(string), Description = "Basket Id")]
        [OpenApiParameter(name: "X-Forwarded-For", In = ParameterLocation.Header, Required = true, Type = typeof(string), Description = "IP Address")]
        [OpenApiParameter(name: "User-Agent", In = ParameterLocation.Header, Required = true, Type = typeof(string), Description = "Browser Information")]
        [Function("Payment")]
        public async Task<HttpResponseData> Payment([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Payment")] HttpRequestData req)
        {
            string ipAddress = null;
            string userAgent = null;
            IEnumerable<string> headerValues;

            try
            {
                var basketId = req?.Headers.GetValues("basketId").FirstOrDefault();

                var authToken = Utility.TryGetAuthorizationToken(req.Headers);

                if (req?.Headers.TryGetValues("X-Forwarded-For", out headerValues) == true)
                {
                    var ipAddresses = headerValues.FirstOrDefault().Split(",");
                    ipAddress = ipAddresses.Length > 0 ? ipAddresses[0] : headerValues.FirstOrDefault();
                }

                if (req?.Headers.TryGetValues("User-Agent", out headerValues) == true)
                {
                    userAgent = headerValues.FirstOrDefault();
                }

                //Temporary Logging to see the X-Forwarded-For header behaviour
                _logger.LogInformation($"IPAddress: {ipAddress}, Headers: {req?.Headers}");

                var result = await _mediator.Send(new PaymentRefIdRequest(basketId, authToken, ipAddress, userAgent, req.Body));

                return await ResultExtention<PaymentResponse>.ExecuteResultAsync(req, CSharpFunctionalExtensions.Result.Success(new Response<PaymentResponse>()
                {
                    Status = Status.OK,
                    Value = result,
                }));
            }
            catch (GraphQLException qex)
            {
                var status = GraphQLExceptionStatus.GetStatusCode(qex);

                var errors = qex.Errors.First().Extensions.Response is not null ?
                    Json.Deserialize<IEnumerable<ErrorDetails>>(qex.Errors.First().Extensions.Response) :
                    ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(qex.Errors?.First()?.Extensions?.Code), AsyncContext.Current.CorrelationId.ToString(), Constants.Application.ErrorFilePath);

                return await ResultExtention<PaymentResponse>.ExecuteResultAsync(req, CSharpFunctionalExtensions.Result.Success(new Response<PaymentResponse>()
                {
                    Status = status,
                    Errors = errors.ToList(),
                    IgnoreNullValues = true
                }));
            }
        }

        [OpenApiOperation(operationId: "PaymentAmount", tags: new[] { "Payment" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "BasketId", In = ParameterLocation.Header, Required = true, Type = typeof(string), Description = "Basket Id")]
        [OpenApiParameter(name: "X-FORWARDED-FOR", In = ParameterLocation.Header, Required = true, Type = typeof(string), Description = "IP Address")]
        [OpenApiParameter(name: "User-Agent", In = ParameterLocation.Header, Required = true, Type = typeof(string), Description = "Browser Information")]
        [OpenApiRequestBody("application/json", typeof(PaymentAmountRequest), Description = "")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(PaymentResponse), Summary = "OK", Description = "OK")]
        [Function("PaymentAmount")]
        public async Task<HttpResponseData> PaymentAmount([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Payment/Amount")] HttpRequestData req)
        {
            string ipAddress = null;
            string userAgent = null;
            IEnumerable<string> headerValues;

            try
            {
                var basketId = req?.Headers.GetValues("basketId").FirstOrDefault();

                var authToken = Utility.TryGetAuthorizationToken(req.Headers);

                if (req?.Headers.TryGetValues("X-FORWARDED-FOR", out headerValues) == true)
                {
                    ipAddress = headerValues.FirstOrDefault();
                }
                if (req?.Headers.TryGetValues("User-Agent", out headerValues) == true)
                {
                    userAgent = headerValues.FirstOrDefault();
                }

                var result = await _mediator.Send(new PaymentAmountRequest(basketId, authToken, ipAddress, userAgent, req.Body));
                return await ResultExtention<PaymentResponse>.ExecuteResultAsync(req, CSharpFunctionalExtensions.Result.Success(new Response<PaymentResponse>()
                {
                    Status = Status.OK,
                    Value = result,
                }));
            }
            catch (GraphQLException qex)
            {
                var status = GraphQLExceptionStatus.GetStatusCode(qex);

                var errors = qex.Errors.First().Extensions.Response is not null ?
                    Json.Deserialize<IEnumerable<ErrorDetails>>(qex.Errors.First().Extensions.Response) :
                    ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(qex.Errors?.First()?.Extensions?.Code), AsyncContext.Current.CorrelationId.ToString(), Constants.Application.ErrorFilePath);

                return await ResultExtention<PaymentResponse>.ExecuteResultAsync(req, CSharpFunctionalExtensions.Result.Success(new Response<PaymentResponse>()
                {
                    Status = status,
                    Errors = errors.ToList(),
                    IgnoreNullValues = true
                }));
            }
        }

        [OpenApiOperation(operationId: "PaymentStatus", tags: new[] { "PaymentStatus" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(ConfirmationResponse), Summary = "OK", Description = "OK")]
        [OpenApiParameter(name: "BasketId", In = ParameterLocation.Header, Required = true, Type = typeof(string), Description = "Basket Id")]
        [Function("PaymentStatus")]
        public async Task<HttpResponseData> PaymentStatus([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "Payment/PaymentStatus")] HttpRequestData req)
        {
            try
            {
                var basketId = req?.Headers.GetValues("basketId").FirstOrDefault();
                var authToken = Utility.TryGetAuthorizationToken(req.Headers);

                var result = await _mediator.Send(new PaymentStatusRequest(basketId, authToken, req.Body));

                return await ResultExtention<PaymentStatusResponse>.ExecuteResultAsync(req, CSharpFunctionalExtensions.Result.Success(new Response<PaymentStatusResponse>()
                {
                    Status = Status.OK,
                    Value = result,
                }));
            }
            catch (GraphQLException qex)
            {
                var status = GraphQLExceptionStatus.GetStatusCode(qex);

                var errors = qex.Errors.First().Extensions.Response is not null ?
                    Json.Deserialize<IEnumerable<ErrorDetails>>(qex.Errors.First().Extensions.Response) :
                    ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(qex.Errors?.First()?.Extensions?.Code), AsyncContext.Current.CorrelationId.ToString(), Constants.Application.ErrorFilePath);

                return await ResultExtention<PaymentStatusResponse>.ExecuteResultAsync(req, CSharpFunctionalExtensions.Result.Success(new Response<PaymentStatusResponse>()
                {
                    Status = status,
                    Errors = errors.ToList(),
                    IgnoreNullValues = true
                }));
            }
        }


        [OpenApiOperation(operationId: "RetrievePayment", tags: new[] { "RetrievePayment" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(ConfirmationResponse), Summary = "OK", Description = "OK")]
        [OpenApiParameter(name: "BasketId", In = ParameterLocation.Header, Required = true, Type = typeof(string), Description = "Basket Id")]
        [Function("RetrievePayment")]
        public async Task<HttpResponseData> RetrievePayment([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Payment")] HttpRequestData req)
        {
            try
            {
                var basketId = req?.Headers.GetValues("basketId").FirstOrDefault();
                var authToken = Utility.TryGetAuthorizationToken(req.Headers);

                var result = await _mediator.Send(new PaymentDetailsResponseRequest(basketId, authToken));

                return await ResultExtention<PaymentResponse>.ExecuteResultAsync(req, CSharpFunctionalExtensions.Result.Success(new Response<PaymentResponse>()
                {
                    Status = Status.OK,
                    Value = result,
                }));
            }
            catch (GraphQLException qex)
            {
                var status = GraphQLExceptionStatus.GetStatusCode(qex);

                var errors = qex.Errors.First().Extensions.Response is not null ?
                    Json.Deserialize<IEnumerable<ErrorDetails>>(qex.Errors.First().Extensions.Response) :
                    ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(qex.Errors?.First()?.Extensions?.Code), AsyncContext.Current.CorrelationId.ToString(), Constants.Application.ErrorFilePath);

                return await ResultExtention<PaymentResponse>.ExecuteResultAsync(req, CSharpFunctionalExtensions.Result.Success(new Response<PaymentResponse>()
                {
                    Status = status,
                    Errors = errors.ToList(),
                    IgnoreNullValues = true
                }));
            }
        }
    }
}
