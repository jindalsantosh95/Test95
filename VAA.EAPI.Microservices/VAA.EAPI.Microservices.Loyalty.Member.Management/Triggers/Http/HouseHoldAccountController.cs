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
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Interface;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.HoHAccount;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Triggers.Http
{
    public class HouseHoldAccountController
    {
        private readonly ILogger<HouseHoldAccountController> _logger;
        private readonly IHouseHoldAccountLinkingCommand _houseHoldAccountLinkingCommandHandler;
        private readonly IHouseholdDetailsRetrievalQuery _householdDetailsRetrievalQueryHandler;
        private readonly IMediator _mediator;

        public HouseHoldAccountController(IMediator mediator, ILogger<HouseHoldAccountController> logger, IHouseHoldAccountLinkingCommand houseHoldAccountLinkingCommandHandler, IHouseholdDetailsRetrievalQuery householdDetailsRetrievalQueryHandler)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _houseHoldAccountLinkingCommandHandler = houseHoldAccountLinkingCommandHandler ?? throw new ArgumentNullException(nameof(houseHoldAccountLinkingCommandHandler));
            _householdDetailsRetrievalQueryHandler = householdDetailsRetrievalQueryHandler ?? throw new ArgumentNullException(nameof(householdDetailsRetrievalQueryHandler));
        }

        [OpenApiOperation(operationId: "LinkAccount", tags: new[] { "Loyalty Member Management Microservice - household/LinkAccount" }, Summary = "Submit a new request for House Hold Account Linking", Description = "http endpoint to submit a request for HoH account linking.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(HouseHoldAccountLinkingRequest), Description = "Link Account Details")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Accepted, CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "Request accepted", Description = "Request accepted")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("LinkAccount")]
        public async Task<HttpResponseData> LinkAccountAsync([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "household/LinkAccount")] HttpRequestData req, CancellationToken cancellationToken)
        {
            try
            {
                var request = await new StreamReader(req.Body).ReadToEndAsync();
                var linkHoHAccountRequest = Json.Deserialize<HouseHoldAccountLinkingCommand>(request);
                var commandOutput = await _houseHoldAccountLinkingCommandHandler.Handle(linkHoHAccountRequest, cancellationToken);
                return await ResultExtention<HouseHoldAccountLinkingResponse>.ExecuteResultAsync(req, commandOutput);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error occurred in LinkAccountAsync");

                var response = req.CreateResponse(HttpStatusCode.InternalServerError);

                await response.WriteAsJsonAsync(new List<ErrorDetails>
                {
                    new()
                    {
                        Message = "An unexpected error occurred."
                    }
                });

                return response;
            }
        }
        [OpenApiOperation(operationId: "DelinkAccount", tags: new[] { "Loyalty Member Management Microservice - household/DelinkAccount" }, Summary = "Submit a new request for House Hold Account Delinking", Description = "http endpoint to submit a request for HoH account Delinking.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(HouseHoldAccountDelinkingRequest), Description = "Delink Account Details")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Accepted, CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "Request accepted", Description = "Request accepted")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("DelinkAccount")]
        public async Task<HttpResponseData> DelinkAccountAsync([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "household/DelinkAccount")] HttpRequestData req)
        {
            _logger.LogInformation("DelinkAccount process started");
            try
            {
                var request = await new StreamReader(req.Body).ReadToEndAsync();
                var delinkRequest = Json.Deserialize<HouseHoldAccountDelinkingRequest>(request);
                var commandOutput = await _mediator.Send(new HouseHoldAccountDelinkingCommand()
                {
                    InitiatedBy = delinkRequest.InitiatedBy,
                    HeadAccount = delinkRequest.HeadAccount,
                    ChildAccount = delinkRequest.ChildAccount
                });
                return await ResultExtention<HouseHoldAccountDelinkingResponse>.ExecuteResultAsync(req, commandOutput);
            }
            finally
            {
                _logger.LogInformation("DelinkAccount process ended");
            }
        }

        [Function("HouseholdDetails")]
        [OpenApiOperation(operationId: "HouseholdDetails", tags: new[] { "Loyalty Member Management Microservice - Household Details Retrieval" }, Summary = "A new query for Household Details retrieval", Description = "http endpoint to retrieve Household details. HOH, Household Members & Household History details are returned in response", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiParameter(name: "customerId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Customer ID")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(HouseholdDetailsResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]

        public async Task<HttpResponseData> HouseholdDetails([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "HouseholdDetails/{customerId}")] HttpRequestData req, string customerId, CancellationToken cancellationToken)
        {
            try
            {
                var commandOutput = await _householdDetailsRetrievalQueryHandler.Handle(new HouseholdDetailsRetrievalQuery { CustomerId = customerId }, cancellationToken);
                return await ResultExtention<HouseholdDetailsResponse>.ExecuteResultAsync(req, commandOutput);
            }
            catch (Exception ex)
            { 
                var response = req.CreateResponse(HttpStatusCode.InternalServerError);

                await response.WriteAsJsonAsync(new List<ErrorDetails>
                {
                    new()
                    {
                        Message = "An unexpected error occurred."
                    }
                });

                return response;
            }

        }

        [Function("HouseHoldAccountValidation")]
        [OpenApiOperation(operationId: "HouseHoldAccountValidation", tags: new[] { "Loyalty Member Microservice - Household Account Validation" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody("application/json", typeof(HouseHoldAccountValidationCommand), Description = "")]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(HouseHoldAccountValidationResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(HouseHoldAccountValidationResponse), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(HouseHoldAccountValidationResponse), Summary = "Unauthorized", Description = "Unauthorized")]

        public async Task<HttpResponseData> HouseHoldAccountValidation([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "HouseHoldAccountValidation")] HttpRequestData req)
        {
            return await ResultExtention<string>.ExecuteResultAsync(req, await _mediator.Send(Json.Deserialize<HouseHoldAccountValidationCommand>(await new StreamReader(req.Body).ReadToEndAsync()), CancellationToken.None));
        }
    }
}
