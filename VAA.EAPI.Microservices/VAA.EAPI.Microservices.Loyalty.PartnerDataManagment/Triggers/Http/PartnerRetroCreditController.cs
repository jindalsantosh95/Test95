using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System.Net;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.Command.RetroCredit;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.RetroCredit;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Helper;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Triggers.Http
{
    public class PartnerRetroCreditController
    {
        private readonly IMediator _mediator;
        private readonly ILogger<PartnerRetroCreditController> _logger;

        public PartnerRetroCreditController(IMediator mediator, ILogger<PartnerRetroCreditController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [Function("OnlineRetroCredit")]
        [OpenApiOperation(operationId: "OnlineRetroCredit", tags: new[] { "PDM Microservice" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody(contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(RetroCreditRequest), Required = true, Description = "This should contain the request information for the Retro Credit.")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Accepted, Summary = "Request accepted", Description = "Request accepted")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(RetroCreditResponse), Summary = "Success", Description = "Success")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(RetroCreditResponse), Summary = "Success", Description = "Success")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]

        public async Task<HttpResponseData> OnlineRetroCredit([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "OnlineRetroCredit")] HttpRequestData req)
        {
            RetroCreditRequest skyTeamMemberRetroCreditRequest;
            
            var reqBody = await new StreamReader(req.Body).ReadToEndAsync();
            
            try
            {
                skyTeamMemberRetroCreditRequest = Json.Deserialize<RetroCreditRequest>(reqBody);
                
                var result = await _mediator.Send(new RetroCreditCommand { RetroCreditRequest = skyTeamMemberRetroCreditRequest }, CancellationToken.None);
                
                return await ResponseDataHelper.CreateResponse(req, result);
            }
            
            catch (Exception ex)
            {
                _logger.LogError(ex, $"PartnerRetroCreditController-OnlineRetroCredit - request {reqBody} - failed due to Exception");
                
                return await ResultExtention<RetroCreditResponse>.ExecuteResultAsync(req,
                 new Response<RetroCreditResponse>
                 {
                     Status = Status.Error,
                     Errors = new List<ErrorDetails>
                    {
                        new ErrorDetails
                        {
                             Message = Constants.ErrorCodes.Tech_Error,
                             ErrorSource = ex.Message,
                        }
                    }
                 });
            }
        }


        [Function("OnlineRetroCreditFeedback")]
        [OpenApiOperation(operationId: "OnlineRetroCreditFeedback", tags: new[] { "PDM Microservice" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody(contentType: System.Net.Mime.MediaTypeNames.Application.Json, bodyType: typeof(RetroFeedbackRequest), Required = true, Description = "This should contain the request information for the Retro Credit Feedback.")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Accepted, Summary = "Request accepted", Description = "Request accepted")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(RetroFeedbackResponse), Summary = "Success", Description = "Success")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(RetroFeedbackResponse), Summary = "Success", Description = "Success")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]

        public async Task<HttpResponseData> OnlineRetroCreditFeedback([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "OnlineRetroCreditFeedback")] HttpRequestData req)
        {
            RetroFeedbackRequest skyTeamMemberRetroFeedbackRequest;
            var reqBody = await new StreamReader(req.Body).ReadToEndAsync();
            
            try
            {
                skyTeamMemberRetroFeedbackRequest = Json.Deserialize<RetroFeedbackRequest>(reqBody);
                
                var result = await _mediator.Send(new RetroFeedbackCommand { RetroFeedbackRequest = skyTeamMemberRetroFeedbackRequest }, CancellationToken.None);
                
                return await ResponseDataHelper.CreateResponse(req, result);
            }
            
            catch (Exception ex)
            {
                _logger.LogError(ex, $"PartnerRetroCreditController-OnlineRetroFeedback - request {reqBody} - failed due to Exception");
                
                return await ResultExtention<RetroFeedbackResponse>.ExecuteResultAsync(req,
                 new Response<RetroFeedbackResponse>
                 {
                     Status = Status.Error,
                     Errors = new List<ErrorDetails>
                    {
                        new ErrorDetails
                        {
                             Message = Constants.ErrorCodes.Tech_Error,
                             ErrorSource = ex.Message,
                        }
                    }
                 });

            }
        }
    }
}
