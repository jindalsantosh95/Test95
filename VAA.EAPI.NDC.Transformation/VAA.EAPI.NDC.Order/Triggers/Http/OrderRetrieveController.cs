using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.NDC.Order.Application.OrderRetrieve;
using VAA.EAPI.NDC.Order.Domain.OrderRetrieveRQ;
using VAA.EAPI.NDC.Order.Domain.OrderViewRS;


namespace VAA.EAPI.NDC.Order.Triggers.Http
{
    public class OrderRetrieveController
    {
        private readonly IMediator _mediator;

        public OrderRetrieveController(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [OpenApiOperation(operationId: "OrderRetrieve", tags: new[] { "Order Retrieve" }, Summary = "This endpoint is used to retrieve PNR", Description = "This endpoint is used to retrieve PNR", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody("application/xml", typeof(IATA_OrderRetrieveRQ), Description = "")]
        [OpenApiParameter(name: "channel", In = ParameterLocation.Header, Required = true, Type = typeof(string), Description = "Channel id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: System.Net.Mime.MediaTypeNames.Application.Xml, bodyType: typeof(IATA_OrderViewRS), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "OK", Description = "OK")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/xml", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/xml", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [Function("OrderRetrieve")]
        public async Task<HttpResponseData> OrderRetrieve([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "OrderRetrieve")] HttpRequestData req)
        {
            var orderRetrieveReq = Xml.Deserialize<IATA_OrderRetrieveRQ>(await new StreamReader(req.Body).ReadToEndAsync());
            var queryOutput = await _mediator.Send(new OrderRetrieveQuery() { OrderRetrieveRQ = orderRetrieveReq, SubscriptionKey = req.Headers?.GetValues("Ocp-Apim-Subscription-Key").FirstOrDefault() }, CancellationToken.None);

            return await ResultExtention<IATA_OrderViewRS>.ExecuteResultAsync(req, queryOutput, true);
        }
    }
}
