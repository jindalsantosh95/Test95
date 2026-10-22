using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.Retail.Reference.Application.Query;
using VAA.EAPI.Microservices.Retail.Reference.Domain;

namespace VAA.EAPI.Microservices.Retail.Reference.Triggers
{
    public class LoyaltyDataController
    {
        private readonly IMediator _mediator;

        public LoyaltyDataController(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [OpenApiOperation(operationId: "BlackListedEmails", tags: new[] { "Reference data Microservice" }, Summary = "Fetch black listed emails data for Manage Account", Description = "Endpoint to fetch black listed emails data", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(IEnumerable<BlackListedEmails>), Summary = "Fetch blacklisted emails", Description = "Fetch blacklisted during enrolment and managing account")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("BlackListedEmails")]
        public async Task<HttpResponseData> GetBlackListedEmails([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "BlackListedEmails")] HttpRequestData req)
        {
            var response = Result.Success(new Response<IEnumerable<BlackListedEmails>>
            {
                Status = Status.OK,
                Value = new List<BlackListedEmails> { new() { Emails = await _mediator.Send(new BlackListedEmailQuery()) } }
            });

            return await ResultExtention<IEnumerable<BlackListedEmails>>.ExecuteResultAsync(req, response);
        }
    }
}
