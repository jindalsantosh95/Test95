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
using System.Threading.Tasks;
using VAA.EAPI.Web.Book.Helper;
using VAA.EAPI.Web.Book.Models.Book.Confirmation;
using VAA.EAPI.Web.Common;
using VAA.EAPI.Web.Common.Entities;
using VAA.EAPI.Web.Common.Extensions;
using VAA.EAPI.Web.Common.Helper;
using VAA.EAPI.Web.Common.Infrastructure.Exceptions;
using VAA.EAPI.Web.Common.Serialisation;

namespace VAA.EAPI.Web.Book.Trigger.Http
{
    public class ConfirmationController
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ConfirmationController> _logger;

        public ConfirmationController(IMediator mediator, ILogger<ConfirmationController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [OpenApiOperation(operationId: "Confirmation", tags: new[] { "Confirmation" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody("application/json", typeof(OrderConfirmation), Description = "Order Confirmation Request")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(ConfirmationResponse), Summary = "OK", Description = "OK")]
        [OpenApiParameter(name: "BasketId", In = ParameterLocation.Header, Required = true, Type = typeof(string), Description = "Basket Id")]
        [OpenApiParameter(name: "X-Forwarded-For", In = ParameterLocation.Header, Required = true, Type = typeof(string), Description = "Basket Id")]
        [OpenApiParameter(name: "User-Agent", In = ParameterLocation.Header, Required = true, Type = typeof(string), Description = "Basket Id")]
        [Function("Confirmation")]
        public async Task<HttpResponseData> Confirmation([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Confirmation")] HttpRequestData req)
        {
            string ipAddress = null;
            string userAgent = null;
            IEnumerable<string> headerValues;
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

            var reqBody = await new StreamReader(req.Body).ReadToEndAsync();

            if (!reqBody.IsNullOrEmpty())
            {
                ConfirmationRequest request = null;
                var reqObject = Json.Deserialize<OrderConfirmation>(reqBody);
                if (reqObject.CustomerConsentInfo != null && !reqObject.MetaPartner.IsNullOrEmpty())
                {
                    request = new ConfirmationRequest(basketId, authToken, "Authorized", ipAddress, userAgent, reqObject.CustomerConsentInfo != null && reqObject.CustomerConsentInfo.CreditCardCheckAccepted, reqObject.MetaPartner);
                }
                else if (reqObject.CustomerConsentInfo == null && !reqObject.MetaPartner.IsNullOrEmpty())
                {
                    request = new ConfirmationRequest(basketId, authToken, "Authorized", ipAddress, userAgent, reqObject.MetaPartner);
                }
                else 
                {
                    request = new ConfirmationRequest(basketId, authToken, "Authorized", ipAddress, userAgent, reqObject.CustomerConsentInfo != null && reqObject.CustomerConsentInfo.CreditCardCheckAccepted);
                }

                var result = await _mediator.Send(request);

                return await ResultExtention<ConfirmationResponse>.ExecuteResultAsync(req, CSharpFunctionalExtensions.Result.Success(result));
            }
            else
            {
                var result = await _mediator.Send(new ConfirmationRequest(basketId, authToken, "Authorized", ipAddress, userAgent));

                return await ResultExtention<ConfirmationResponse>.ExecuteResultAsync(req, CSharpFunctionalExtensions.Result.Success(result));
            }
        }

        [OpenApiOperation(operationId: "RetrieveConfirmation", tags: new[] { "RetrieveConfirmation" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(ConfirmationResponse), Summary = "OK", Description = "OK")]
        [OpenApiParameter(name: "BasketId", In = ParameterLocation.Header, Required = true, Type = typeof(string), Description = "Basket Id")]
        [Function("RetrieveConfirmation")]
        public async Task<HttpResponseData> RetrieveConfirmation([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Confirmation")] HttpRequestData req)
        {
            try
            {
                var basketId = req?.Headers.GetValues("basketId").FirstOrDefault();
                var authToken = Utility.TryGetAuthorizationToken(req.Headers);

                var result = await _mediator.Send(new OrderConfirmationResponseRequest(basketId, authToken));

                return await ResultExtention<ConfirmationResponse>.ExecuteResultAsync(req, CSharpFunctionalExtensions.Result.Success(new Response<ConfirmationResponse>()
                {
                    Status = Status.OK,
                    Value = result,
                }));
            }
            catch (GraphQLException gqlException)
            {              

                var status = GraphQLExceptionStatus.GetStatusCode(gqlException);

                var errorResponse = gqlException.Errors.FirstOrDefault().Extensions.Response is not null ?
                    Json.Deserialize<IEnumerable<ErrorDetails>>(gqlException.Errors.FirstOrDefault().Extensions.Response) :
                    ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(gqlException.Errors?.FirstOrDefault()?.Extensions?.Code), AsyncContext.Current.CorrelationId.ToString(), Constants.Application.ErrorFilePath);

                return await ResultExtention<ConfirmationResponse>.ExecuteResultAsync(req, CSharpFunctionalExtensions.Result.Success(new Response<ConfirmationResponse>()
                {
                    Status = status,
                    Errors = errorResponse.ToList(),
                    IgnoreNullValues = true
                }));               
            }            

        }
    }
}
