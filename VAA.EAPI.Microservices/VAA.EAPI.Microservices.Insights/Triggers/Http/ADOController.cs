using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Insights.Application.Providers;
using VAA.EAPI.Microservices.Insights.Interfaces;
using VAA.EAPI.Microservices.Insights.Models;

namespace VAA.EAPI.Microservices.Insights.Triggers.Http
{
    public sealed class ADOController
    {
        private readonly IADODeploymentCompiler _compiler;
        private readonly IADOPipelineProcessor _pipelineProcessor;

        public ADOController(IADODeploymentCompiler compiler, IADOPipelineProcessor pipelineProcessor)
        {
            _compiler = compiler ?? throw new ArgumentNullException(nameof(compiler));
            _pipelineProcessor = pipelineProcessor ?? throw new ArgumentNullException(nameof(pipelineProcessor));
        }

        [OpenApiOperation(operationId: "GetDeploymentStatus", tags: new[] { "ADO Metrics" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(ADODeploymentCollection), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("GetDeploymentStatus")]
        public async Task<HttpResponseData> GetDeploymentStatus([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "deploymentstatus")] HttpRequestData req)
        {
            var statusCollection = await _compiler.GetCompiledData();
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(statusCollection);
            return response;
        }

        [OpenApiOperation(operationId: "CompileDeploymentStatus", tags: new[] { "ADO Metrics" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Accepted, contentType: "application/json", bodyType: typeof(string), Summary = "Accepted", Description = "Accepted")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("CompileDeploymentStatus")]
        public async Task<HttpResponseData> CompileDeploymentStatus([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "deploymentstatus/compile")] HttpRequestData req)
        {
            _ = Task.Run(_compiler.CompileDeploymentDataAsync);
            var response = req.CreateResponse(HttpStatusCode.Accepted);
            await response.WriteAsJsonAsync("Request Accepted");
            return response;
        }

        [OpenApiOperation(operationId: "CompileDeploymentStatusForPipeline", tags: new[] { "ADO Metrics" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(ADODeploymentStatusCompileRequest), Required = true, Description = "Pipeline id and name to compile deployment status.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Accepted, contentType: "application/json", bodyType: typeof(string), Summary = "Accepted", Description = "Accepted")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(string), Summary = "Bad Request", Description = "PipelineId and PipelineName are required.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("CompileDeploymentStatusForPipeline")]
        public async Task<HttpResponseData> CompileDeploymentStatusForPipeline([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "deploymentstatus/compile/pipeline")] HttpRequestData req)
        {
            ADODeploymentStatusCompileRequest request;

            try
            {
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                request = JsonConvert.DeserializeObject<ADODeploymentStatusCompileRequest>(requestBody);
            }
            catch (Exception ex) when (ex is JsonSerializationException || ex is JsonReaderException)
            {
                var badJsonResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badJsonResponse.WriteStringAsync($"Invalid JSON format: {ex.Message}");
                return badJsonResponse;
            }

            if (request == null || request.PipelineId <= 0)
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteStringAsync("PipelineId is required.");
                return badRequest;
            }

            if (string.IsNullOrWhiteSpace(request.PipelineName))
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteStringAsync("PipelineName is required.");
                return badRequest;
            }

            await _pipelineProcessor.HandleAsync(request.PipelineId, request.PipelineName);

            var response = req.CreateResponse(HttpStatusCode.Accepted);
            await response.WriteStringAsync("Request Accepted");
            return response;
        }

        [OpenApiOperation(operationId: "Reset", tags: new[] { "ADO Metrics" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Accepted, contentType: "application/json", bodyType: typeof(string), Summary = "Accepted", Description = "Accepted")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("Reset")]
        public async Task<HttpResponseData> Reset([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "deploymentstatus/reset")] HttpRequestData req)
        {
            _ = Task.Run(_compiler.Reset);
            var response = req.CreateResponse(HttpStatusCode.Accepted);
            await response.WriteAsJsonAsync("Request Accepted");
            return response;
        }
    }
}
