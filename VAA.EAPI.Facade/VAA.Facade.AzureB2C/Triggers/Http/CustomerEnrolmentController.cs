using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using VAA.Facade.AzureB2C.Application.Command;
using VAA.Facade.AzureB2C.Application.Validations;
using VAA.Facade.AzureB2C.Domain.Entities;
using VAA.Facade.AzureB2C.Domain.Model;
using VAA.Facade.AzureB2C.Shared;

namespace VAA.Facade.AzureB2C.Triggers.Http
{
    public class CustomerEnrolmentController
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CustomerEnrolmentController> _logger;

        public CustomerEnrolmentController(IMediator mediator, ILogger<CustomerEnrolmentController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [OpenApiOperation(operationId: "CustomerEnrolmentB2CFacade", tags: new[] { "CustomerEnrolmentB2CFacade" }, Summary = "Reads B2C signup request and call customer enrolment API", Description = "Http endpoint for B2C API Connector. Request has dynamic data fields. ", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody("application/json", typeof(B2CRequest), Description = "static properties are step and identities, other properties can be added")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(B2CResponse), Summary = "The response when from enrolment API", Description = "This returns the response")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<FailureResponse>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<FailureResponse>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("CustomerEnrolmentB2CFacade")]
        public async Task<HttpResponseData> EnrollCustomer([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "customer/enrolmentB2CFacade")] HttpRequestData req, FunctionContext executionContext)
        {
            _logger.LogInformation("Customer Enrolment Facade API Controller");

            string actualRoot = ReadConfig.GetFunctionAppDirRootPath();
            Constants.FunctionAppDirPath = actualRoot;
            // Get the request body
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            // Print out the request body Enable only in dev
            _logger.LogInformation("Request body: " + requestBody);
            var IsValidRequest = await _mediator.Send(new ValidateB2CRequest(requestBody), CancellationToken.None);
            if (!IsValidRequest)
            {
                _logger.LogInformation("Request body: " + requestBody);
                return await ResultExtention<B2CResponse>.ExecuteResultAsync(req, new Response<B2CResponse>()
                {
                    Status = Status.BusinessException,
                    Value = new B2CResponse(Constants.B2CActionValidation, "Mandatory Information Missing")
                });
            }
            var result = await _mediator.Send(new B2CRequestCommand(requestBody), CancellationToken.None);

//            _logger.LogInformation("Response body: " + JsonConvert.SerializeObject(result.Value.Value));
            
            return await ResultExtention<B2CResponse>.ExecuteResultAsync(req, result);

        }
    }
}
