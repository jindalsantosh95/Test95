using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.Facade.NDC.IFGFacade.Application.Command;
using VAA.Facade.NDC.IFGFacade.Domain;

namespace VAA.Facade.NDC.IFGFacade.Triggers.Http
{
    public class IFGFacadeController(IAuthorizeCommand authorizeCommand, IOrderCommand orderCommand, IRefundCommand refundCommand)
    {
        private readonly IAuthorizeCommand _authorizeCommand = authorizeCommand;
        private readonly IOrderCommand _orderCommand = orderCommand;
        private readonly IRefundCommand _refundCommand = refundCommand;
        private readonly CancellationToken _cancellationToken = new();

        [OpenApiOperation(operationId: "Authorize", tags: ["Authorize Payment Facade layer to call retail Payment MS"], Summary = "Provides ability to interact with retail Payment MS authorize endpoint", Description = "Http endpoint to interact with retail Payment MS authorize endpoint", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody(contentType: MediaTypeNames.Application.Json, bodyType: typeof(AuthorizeRequest), Required = true, Description = "")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: MediaTypeNames.Application.Json, bodyType: typeof(PaymentResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Bad Request", Description = "Bad Request")]
        [Function("Authorize")]
        public async Task<HttpResponseData> Authorize([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Authorize")] HttpRequestData req)
        {
            var request = await new StreamReader(req.Body).ReadToEndAsync();
            var authorizeRequest = Json.Deserialize<AuthorizeRequest>(request);
            var commandOutput = await _authorizeCommand.Handle(authorizeRequest, _cancellationToken);

            return await ResultExtention<PaymentResponse>.ExecuteResultAsync(req, commandOutput);
        }

        [OpenApiOperation(operationId: "Order", tags: ["Order Payment Facade layer to call retail Payment MS"], Summary = "Provides ability to interact with retail Payment MS capture endpoint", Description = "Http endpoint to interact with retail Payment MS capture endpoint", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody(contentType: MediaTypeNames.Application.Json, bodyType: typeof(OrderRequest), Required = true, Description = "")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: MediaTypeNames.Application.Json, bodyType: typeof(PaymentResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Bad Request", Description = "Bad Request")]
        [Function("Order")]
        public async Task<HttpResponseData> Order([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Order")] HttpRequestData req)
        {
            var request = await new StreamReader(req.Body).ReadToEndAsync();
            var orderPayment = Json.Deserialize<OrderRequest>(request);
            var commandOutput = await _orderCommand.Handle(orderPayment, _cancellationToken);

            return await ResultExtention<PaymentResponse>.ExecuteResultAsync(req, commandOutput);
        }

        [OpenApiOperation(operationId: "Refund", tags: ["Refund Payment Facade layer to call retail Payment MS"], Summary = "Provides ability to interact with retail Payment MS capture endpoint", Description = "Http endpoint to interact with retail Payment MS Refund endpoint", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody(contentType: MediaTypeNames.Application.Json, bodyType: typeof(RefundRequest), Required = true, Description = "")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: MediaTypeNames.Application.Json, bodyType: typeof(PaymentResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: MediaTypeNames.Application.Json, bodyType: typeof(List<ErrorDetails>), Summary = "Bad Request", Description = "Bad Request")]
        [Function("Refund")]
        public async Task<HttpResponseData> Refund([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Refund")] HttpRequestData req)
        {
            var request = await new StreamReader(req.Body).ReadToEndAsync();
            var refundPayment = Json.Deserialize<RefundRequest>(request);
            var commandOutput = await _refundCommand.Handle(refundPayment, _cancellationToken);

            return await ResultExtention<PaymentResponse>.ExecuteResultAsync(req, commandOutput);
        }
    }
}
