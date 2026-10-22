using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Insights.Application.GitHub;
using VAA.EAPI.Microservices.Insights.Interfaces;
using VAA.EAPI.Microservices.Insights.Models.GitHub;

namespace VAA.EAPI.Microservices.Insights.Triggers.Http
{
    public sealed class GitHubController
    {
        private readonly IGitHubMergedPullRequestQueryHandler _queryHandler;

        public GitHubController(IGitHubMergedPullRequestQueryHandler queryHandler)
        {
            _queryHandler = queryHandler ?? throw new ArgumentNullException(nameof(queryHandler));
        }

        [OpenApiOperation(operationId: "GetMergedPullRequests", tags: new[] { "GitHub" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(GetMergedPullRequestsRequest), Required = true, Description = "Request body containing PAT token, GitHub owner, and date range.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(IEnumerable<GetMergedPullRequestsResponse>), Summary = "OK", Description = "Repositories available to the PAT token, each with pull requests merged into the identified default branch between the supplied dates.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(string), Summary = "Bad Request", Description = "Invalid date range or request payload.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Unexpected processing error")]
        [Function("GetMergedPullRequests")]
        public async Task<HttpResponseData> GetMergedPullRequests([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "github/pullrequests/merged")] HttpRequestData req)
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            GetMergedPullRequestsRequest request;
            try
            {
                request = JsonConvert.DeserializeObject<GetMergedPullRequestsRequest>(requestBody);
            }
            catch (Exception ex) when (ex is JsonSerializationException || ex is JsonReaderException)
            {
                var badJsonResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badJsonResponse.WriteStringAsync($"Invalid JSON format: {ex.Message}");
                return badJsonResponse;
            }

            if (request == null || string.IsNullOrWhiteSpace(request.PersonalAccessToken))
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteStringAsync("PersonalAccessToken is required.");
                return badRequest;
            }

            if (string.IsNullOrWhiteSpace(request.Owner))
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteStringAsync("Owner is required.");
                return badRequest;
            }

            GitHubMergedPullRequestQuery query;

            try
            {
                query = (GitHubMergedPullRequestQuery)request;
            }
            catch (FormatException ex)
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteStringAsync(ex.Message);
                return badRequest;
            }

            IReadOnlyCollection<Domain.GitHub.GitHubRepositoryMergedPullRequests> repositoriesWithMergedPullRequests;

            try
            {
                repositoriesWithMergedPullRequests = await _queryHandler.HandleAsync(query);
            }
            catch (ArgumentException ex)
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteStringAsync(ex.Message);
                return badRequest;
            }

            try
            {
                var responseModel = repositoriesWithMergedPullRequests
                    .Select(repository => (GetMergedPullRequestsResponse)repository)
                    .ToList();

                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(responseModel);
                return response;
            }
            catch
            {
                var errorDetails = new List<ErrorDetails>
                {
                    new ErrorDetails
                    {
                        Code = "InternalServerError",
                        Message = "An unexpected error occurred while processing the request."
                    }
                };
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteAsJsonAsync(errorDetails);
                return errorResponse;
            }
        }
    }
}
