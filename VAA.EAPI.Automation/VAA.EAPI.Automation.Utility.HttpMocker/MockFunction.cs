using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using VAA.EAPI.Automation.Utility.HttpMocker.Application;

namespace VAA.EAPI.Automation.Utility.HttpMocker
{
    public sealed class MockFunction(IMockStorageService mockStorageService)
    {
        private readonly IMockStorageService _mockStorageService = mockStorageService;

        [OpenApiOperation(operationId: "MockApi", tags: ["Http Mock µService"], Summary = "Enables mocking of http response to configured requests", Description = "Enables mocking of http response to configured requests", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: MediaTypeNames.Application.Json, bodyType: typeof(string), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: MediaTypeNames.Application.Xml, bodyType: typeof(string), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: MediaTypeNames.Application.Soap, bodyType: typeof(string), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotImplemented, contentType: "application/json", bodyType: typeof(string), Summary = "Not implemented error", Description = "Occurs when a request has no mapped response")]
        [Function("MockApi")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get", "put", "post", "patch", "delete", "options", Route = "{*requestUri}")] HttpRequest req, string requestUri)
            => new ObjectResult("Not Implemented")
            {
                StatusCode = StatusCodes.Status501NotImplemented
            };

        [OpenApiOperation(operationId: "RunGet", tags: ["HTTP Mock µService"], Summary = "Enables mocking of HTTP response to configured requests", Description = "Enables mocking of HTTP response to configured requests", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: MediaTypeNames.Application.Json, bodyType: typeof(string), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: MediaTypeNames.Application.Xml, bodyType: typeof(string), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: MediaTypeNames.Application.Soap, bodyType: typeof(string), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotImplemented, contentType: "application/json", bodyType: typeof(string), Summary = "Not implemented error", Description = "Occurs when a request has no mapped response")]
        [Function("RunGet")]
        public IActionResult RunGet([HttpTrigger(AuthorizationLevel.Function, "get", Route = "mock/{*requestUri}")] HttpRequest req, string requestUri)
            => new ObjectResult("Not Implemented") { StatusCode = StatusCodes.Status501NotImplemented };

        [OpenApiOperation(operationId: "RunPut", tags: ["HTTP Mock µService"], Summary = "Enables mocking of HTTP response to configured requests", Description = "Enables mocking of HTTP response to configured requests", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: MediaTypeNames.Application.Json, bodyType: typeof(string), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: MediaTypeNames.Application.Xml, bodyType: typeof(string), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: MediaTypeNames.Application.Soap, bodyType: typeof(string), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotImplemented, contentType: "application/json", bodyType: typeof(string), Summary = "Not implemented error", Description = "Occurs when a request has no mapped response")]
        [Function("RunPut")]
        public IActionResult RunPut([HttpTrigger(AuthorizationLevel.Function, "put", Route = "mock/{*requestUri}")] HttpRequest req, string requestUri)
            => new ObjectResult("Not Implemented") { StatusCode = StatusCodes.Status501NotImplemented };

        [OpenApiOperation(operationId: "RunPost", tags: ["HTTP Mock µService"], Summary = "Enables mocking of HTTP response to configured requests", Description = "Enables mocking of HTTP response to configured requests", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: MediaTypeNames.Application.Json, bodyType: typeof(string), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: MediaTypeNames.Application.Xml, bodyType: typeof(string), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: MediaTypeNames.Application.Soap, bodyType: typeof(string), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotImplemented, contentType: "application/json", bodyType: typeof(string), Summary = "Not implemented error", Description = "Occurs when a request has no mapped response")]
        [Function("RunPost")]
        public IActionResult RunPost([HttpTrigger(AuthorizationLevel.Function, "post", Route = "mock/{*requestUri}")] HttpRequest req, string requestUri)
            => new ObjectResult("Not Implemented") { StatusCode = StatusCodes.Status501NotImplemented };

        [OpenApiOperation(operationId: "RunPatch", tags: ["HTTP Mock µService"], Summary = "Enables mocking of HTTP response to configured requests", Description = "Enables mocking of HTTP response to configured requests", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: MediaTypeNames.Application.Json, bodyType: typeof(string), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: MediaTypeNames.Application.Xml, bodyType: typeof(string), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: MediaTypeNames.Application.Soap, bodyType: typeof(string), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotImplemented, contentType: "application/json", bodyType: typeof(string), Summary = "Not implemented error", Description = "Occurs when a request has no mapped response")]
        [Function("RunPatch")]
        public IActionResult RunPatch([HttpTrigger(AuthorizationLevel.Function, "patch", Route = "mock/{*requestUri}")] HttpRequest req, string requestUri)
            => new ObjectResult("Not Implemented") { StatusCode = StatusCodes.Status501NotImplemented };

        [OpenApiOperation(operationId: "RunDelete", tags: ["HTTP Mock µService"], Summary = "Enables mocking of HTTP response to configured requests", Description = "Enables mocking of HTTP response to configured requests", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: MediaTypeNames.Application.Json, bodyType: typeof(string), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: MediaTypeNames.Application.Xml, bodyType: typeof(string), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: MediaTypeNames.Application.Soap, bodyType: typeof(string), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotImplemented, contentType: "application/json", bodyType: typeof(string), Summary = "Not implemented error", Description = "Occurs when a request has no mapped response")]
        [Function("RunDelete")]
        public IActionResult RunDelete([HttpTrigger(AuthorizationLevel.Function, "delete", Route = "mock/{*requestUri}")] HttpRequest req, string requestUri)
            => new ObjectResult("Not Implemented") { StatusCode = StatusCodes.Status501NotImplemented };

        [OpenApiOperation(operationId: "RunOptions", tags: ["HTTP Mock µService"], Summary = "Enables mocking of HTTP response to configured requests", Description = "Enables mocking of HTTP response to configured requests", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: MediaTypeNames.Application.Json, bodyType: typeof(string), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: MediaTypeNames.Application.Xml, bodyType: typeof(string), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: MediaTypeNames.Application.Soap, bodyType: typeof(string), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotImplemented, contentType: "application/json", bodyType: typeof(string), Summary = "Not implemented error", Description = "Occurs when a request has no mapped response")]
        [Function("RunOptions")]
        public IActionResult RunOptions([HttpTrigger(AuthorizationLevel.Function, "options", Route = "mock/{*requestUri}")] HttpRequest req, string requestUri)
            => new ObjectResult("Not Implemented") { StatusCode = StatusCodes.Status501NotImplemented };

        [OpenApiOperation(operationId: "RunHead", tags: ["HTTP Mock µService"], Summary = "Enables mocking of HTTP response to configured requests", Description = "Enables mocking of HTTP response to configured requests", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: MediaTypeNames.Application.Json, bodyType: typeof(string), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: MediaTypeNames.Application.Xml, bodyType: typeof(string), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: MediaTypeNames.Application.Soap, bodyType: typeof(string), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotImplemented, contentType: "application/json", bodyType: typeof(string), Summary = "Not implemented error", Description = "Occurs when a request has no mapped response")]
        [Function("RunHead")]
        public IActionResult RunHead([HttpTrigger(AuthorizationLevel.Function, "head", Route = "mock/{*requestUri}")] HttpRequest req, string requestUri)
            => new ObjectResult("Not Implemented") { StatusCode = StatusCodes.Status501NotImplemented };

        [OpenApiOperation(operationId: "SynchroniseMock", tags: ["HTTP Mock µService"], Summary = "Reloads mock file from storage", Description = "Reloads mock file from storage", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Accepted, contentType: MediaTypeNames.Application.Json, bodyType: typeof(string), Summary = "A request to reload a mock file from storage has been accepted", Description = "A request to reload a mock file from storage has been accepted")]
        [Function("SynchroniseMock")]
        public async Task<IActionResult> SynchroniseMock([HttpTrigger(AuthorizationLevel.Function, "get", Route = "synchronisemock/{*mockName}")] HttpRequest req, string mockName)
            => new ObjectResult((await _mockStorageService.SynchroniseMock(mockName) ? "Synchronised" : "Not Synchronised")) { StatusCode = StatusCodes.Status202Accepted };

        [OpenApiOperation(operationId: "Synchronise", tags: ["HTTP Mock µService"], Summary = "Reloads all mock file(s) from storage", Description = "Reloads all mock file(s) from storage", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Accepted, contentType: MediaTypeNames.Application.Json, bodyType: typeof(string), Summary = "A request to synchronise mock files from storage has been accepted", Description = "A request to synchronise mock files from storage has been accepted")]
        [Function("Synchronise")]
        public IActionResult Synchronise([HttpTrigger(AuthorizationLevel.Function, "get", Route = "synchronise")] HttpRequest req)
        {
            _ = _mockStorageService.Synchronise();

            return new AcceptedResult();
        }
    }
}
