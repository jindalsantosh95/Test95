using MediatR;

namespace VAA.EAPI.Microservices.CheckIn.Triggers.Http
{
    public class CheckInController
    {
        private readonly IMediator _mediator;
        public CheckInController(IMediator mediator)
        {
            _mediator = mediator;
        }

        //[OpenApiOperation(operationId: "OrderChange", tags: new[] { "Order Microservice" }, Summary = "Submit a new event for Order change", Description = "http endpoint to submit a event for Order change which allow asynchronous processing of order change request in background. The status information and response url are returned in the response. Response url further can be used to get the updated order if available,status information otherwise", Visibility = OpenApiVisibilityType.Important)]
        //[OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        //[OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Accepted, CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "Request accepted", Description = "Request accepted")]
        //[OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        //[OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        //[Function("OrderChange")]
        //public async Task<HttpResponseData> OrderChange([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequestData req)
        //{
        //    throw new NotImplementedException();
        //}



        //[OpenApiOperation(operationId: "OrderChangeResponse", tags: new[] { "Order Microservice" }, Summary = "Get order change event response", Description = "It returns order change event response and status against requested corelationId", Visibility = OpenApiVisibilityType.Important)]
        //[OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(OrderChangeResponse), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        //[OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        //[OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        //[Function("OrderChangeResponse")]
        //public async Task<HttpResponseData> OrderChangeResponse([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "order/response/{corelationid}")] HttpRequestData req, string corelationid)
        //{
        //    throw new NotImplementedException();
        //}

    }
}
