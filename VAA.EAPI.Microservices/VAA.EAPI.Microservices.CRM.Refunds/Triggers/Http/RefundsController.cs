using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using System.Net;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.CRM.Refunds.Application.Interfaces;
using VAA.EAPI.Microservices.CRM.Refunds.Models.Refunds;

namespace VAA.EAPI.Microservices.CRM.Refunds.Triggers.Http;

public sealed class RefundsController
{
    private readonly IRefundStatusCommandHandler _getRefundStatusCommandHandler;
    private readonly ISubmitRefundCommandHandler _submitRefundCommandHandler;

    public RefundsController(IRefundStatusCommandHandler getRefundStatusCommandHandler, ISubmitRefundCommandHandler submitRefundCommandHandler)
    {
        _getRefundStatusCommandHandler = getRefundStatusCommandHandler ?? throw new ArgumentNullException(nameof(getRefundStatusCommandHandler));
        _submitRefundCommandHandler = submitRefundCommandHandler ?? throw new ArgumentNullException(nameof(submitRefundCommandHandler));
    }

    [OpenApiOperation(operationId: "GetRefundStatus", tags: new[] { "CRM Refunds" }, Summary = "Returns refund status for a ticket / coupon", Visibility = OpenApiVisibilityType.Important)]
    [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
    [OpenApiRequestBody("application/json", typeof(RefundRequestForm), Required = true, Description = "Refund Request Form")]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(RefundRequestForm), Summary = "Refund status response")]
    [Function("GetRefundStatus")]
    public async Task<HttpResponseData> GetRefundStatus(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "status")] HttpRequestData req,
        CancellationToken cancellationToken)
    {
        // Attempt to deserialize the request body, and if it fails, return a bad request response
        RefundRequestForm request;
        try
        {
            request = await req.ReadFromJsonAsync<RefundRequestForm>(cancellationToken);
        }
        catch (Exception)
        {
            request = null; // If deserialization fails, treat it as a bad request
        }

        // If the request body is missing or invalid, return a bad request response
        if (request is null)
        {
            var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequest.WriteStringAsync("Request body is required.", cancellationToken);
            return badRequest;
        }

        // Process the request and get the refund status
        var responsePayload = await _getRefundStatusCommandHandler.HandleAsync(request, cancellationToken);

        // Return the response as JSON
        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(responsePayload, cancellationToken);

        return response;
    }

    [OpenApiOperation(operationId: "SubmitRefundRequest", tags: new[] { "CRM Refunds" }, Summary = "Submits selected refund entries", Visibility = OpenApiVisibilityType.Important)]
    [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
    [OpenApiRequestBody("application/json", typeof(RefundRequestForm), Required = true, Description = "Refund Request Form")]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(RefundSubmissionResponse), Summary = "D365 reference number")]
    [Function("SubmitRefundRequest")]
    public async Task<HttpResponseData> SubmitRefundRequest(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "submit")] HttpRequestData req,
        CancellationToken cancellationToken)
    {
        RefundRequestForm request;

        try
        {
            request = await req.ReadFromJsonAsync<RefundRequestForm>(cancellationToken);
        }
        catch (Exception)
        {
            request = null; // If deserialization fails, treat it as a bad request
        }

        if (request is null)
        {
            var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequest.WriteStringAsync("Request body is required.", cancellationToken);
            return badRequest;
        }

        try
        {
            var responsePayload = await _submitRefundCommandHandler.HandleAsync(request, cancellationToken);
            HttpResponseData response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(responsePayload, cancellationToken);

            return response;
        }
        catch (InvalidOperationException ex)
        {
            var response = req.CreateResponse(HttpStatusCode.BadRequest);
            await response.WriteStringAsync(ex.Message, cancellationToken);
            return response;
        }
    }
}
