using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using MediatR;
using System.Threading.Tasks;
using System;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using System.Net;
using VAA.EAPI.Domain.Entities.Common;
using System.Collections.Generic;
using VAA.Facade.PDMFacade.Domain;
using Microsoft.OpenApi.Models;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.Facade.PDMFacade.Helper;
using VAA.Facade.PDMFacade.Application.Command;
using System.IO;
using VAA.EAPI.Common.Serialisation;
using System.Threading;

namespace VAA.Facade.PDMFacade.Triggers.Http
{
    public class SkyTeamPdmFacadeController
    {
        private readonly IMediator _mediator;
        private readonly ILogger<SkyTeamPdmFacadeController> _logger;

        public SkyTeamPdmFacadeController(IMediator mediator, ILogger<SkyTeamPdmFacadeController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [OpenApiOperation(operationId: "PDMFacadeRetro", tags: new[] { "PDMFacade" }, Description = "Http endpoint for online retro ", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody(contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(RetroCreditRequest), Required = true, Description = "This should contain the request information for the Retro Credit.")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Accepted, Summary = "Request accepted", Description = "Request accepted")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(RetroCreditResponse), Summary = "Success", Description = "Success")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(RetroCreditResponse), Summary = "Success", Description = "Success")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("OnlineRetroCredit")]
        public async Task<HttpResponseData> OnlineRetroCredit([HttpTrigger(AuthorizationLevel.Anonymous, "POST", Route = "OnlineRetroCredit")] HttpRequestData req,FunctionContext executionContext)
        {
            string mediaType = MediaTypeHelper.SetMediaTypeForRequest(req);
            string reqBody = string.Empty;
            RetroCreditRequest skyTeamMemberRetroCreditRequest;

            try
            {
                reqBody = await new StreamReader(req.Body).ReadToEndAsync();
                skyTeamMemberRetroCreditRequest = mediaType.Equals("xml") ? Xml.Deserialize<RetroCreditRequest>(reqBody) : Json.Deserialize<RetroCreditRequest>(reqBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"PartnerRetroCreditController-OnlineRetroCredit - error - request {reqBody} - failed due to Exception");
                return await MediaTypeHelper.CreateResponseForMediaType<string>(req, mediaType, new Response<string> { Status = Status.BadRequest });
            }

            var result = await _mediator.Send(new RetroCreditCommand() { RetroCreditRequest = skyTeamMemberRetroCreditRequest }, CancellationToken.None);
            return await MediaTypeHelper.CreateResponseForMediaType(req, mediaType, result);


        }

        [OpenApiOperation(operationId: "PDMFacadeFeedback", tags: new[] { "PDMFacade" }, Description = "Http endpoint for retro feedback ", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody(contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(RetroFeedbackRequest), Required = true, Description = "This should contain the request information for the Retro Credit Feedback.")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Accepted, Summary = "Request accepted", Description = "Request accepted")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(RetroFeedbackResponse), Summary = "Success", Description = "Success")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(RetroFeedbackResponse), Summary = "Success", Description = "Success")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("OnlineRetroCreditFeedback")]
        public async Task<HttpResponseData> OnlineRetroCreditFeedback([HttpTrigger(AuthorizationLevel.Anonymous, "POST", Route = "OnlineRetroCreditFeedback")] HttpRequestData req,FunctionContext executionContext)
        {
            string mediaType = MediaTypeHelper.SetMediaTypeForRequest(req);
            string reqBody = string.Empty;
            RetroFeedbackRequest skyTeamMemberRetroFeedbackRequest;

            try
            {
                reqBody = await new StreamReader(req.Body).ReadToEndAsync();
                skyTeamMemberRetroFeedbackRequest = mediaType.Equals("xml") ? Xml.Deserialize<RetroFeedbackRequest>(reqBody) : Json.Deserialize<RetroFeedbackRequest>(reqBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"PartnerRetroCreditController-RetroFeedback - error - request {reqBody} - failed due to Exception");
                return await MediaTypeHelper.CreateResponseForMediaType<string>(req, mediaType, new Response<string> { Status = Status.BadRequest });
            }

           var result= await _mediator.Send(new RetroFeedbackCommand() { RetroFeedbackRequest = skyTeamMemberRetroFeedbackRequest }, CancellationToken.None);
           return await MediaTypeHelper.CreateResponseForMediaType(req, mediaType, result);
        }
    }
}
