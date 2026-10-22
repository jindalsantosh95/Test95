using CSharpFunctionalExtensions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Xml.Linq;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.NDC.Order.Application.OrderChangeNotif;
using VAA.EAPI.NDC.Order.Domain;

namespace VAA.EAPI.NDC.Order.Triggers.Http
{
    public class OrderChangeDuplicatePNRNotifController
    {
        private readonly ILogger<OrderChangeDuplicatePNRNotifController> _logger;
        private readonly IOrderChangeDuplicatePNRNotifCommand _command;

        public OrderChangeDuplicatePNRNotifController(ILogger<OrderChangeDuplicatePNRNotifController> logger, IOrderChangeDuplicatePNRNotifCommand command)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _command = command ?? throw new ArgumentNullException(nameof(command));
        }

        [OpenApiOperation(operationId: "OrderChangeDuplicatePNRNotif", tags: new[] { "OrderChangeDuplicatePNRNotif" }, Summary = "Process Duplicate PNR notification",Description = "Processes duplicate PNR notifications", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody("application/xml", typeof(OrderChangeDuplicatePNRNotifRQ), Description = "Duplicate PNR notification request")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Accepted, CustomHeaderType = typeof(OpenApiCustomResponseHeader),  Summary = "Request Accepted",Description = "Request accepted")]
        [OpenApiSecurity( "OAuth2Client",  SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [Function("OrderChangeDuplicatePNRNotif")]
        public async Task<HttpResponseData> OrderChangeDuplicatePNRNotif( [HttpTrigger( AuthorizationLevel.Anonymous, "post", Route = "OrderChangeDuplicatePNRNotif")]HttpRequestData req)
        {
            var request = await new StreamReader(req.Body).ReadToEndAsync();

            XDocument xml = XDocument.Parse(request);

            var body = xml.Descendants().First(e => e.Name.LocalName == "Body").Elements().First();

            var handleTask = _command.Handle(
                Xml.Deserialize<OrderChangeDuplicatePNRNotifRQ>(body.ToString()),
                System.Threading.CancellationToken.None);

            _ = handleTask.ContinueWith(t =>
            {
                if (t.IsFaulted)
                    _logger.LogError(t.Exception, "Fire-and-forget OCN processing failed.");
                else if (t.IsCanceled)
                    _logger.LogWarning("Fire-and-forget OCN processing was cancelled.");
            }, TaskScheduler.Default);

            var result = Result.Success(new EAPI.Domain.Entities.Common.Response<string>() { Status = EAPI.Domain.Entities.Common.Status.Accepted });

            return await ResultExtention<string>.ExecuteResultAsync(req, result);
        }

        [OpenApiOperation( operationId: "OrderChangeDuplicatePNRNotifValidation",tags: new[] { "OrderChangeDuplicatePNRNotifValidation" }, Summary = "Validation endpoint", Description = "Validation endpoint for Duplicate PNR notification", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithoutBody( statusCode: HttpStatusCode.Accepted, CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "Request Accepted", Description = "Request accepted")]
        [OpenApiSecurity( "OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [Function("OrderChangeDuplicatePNRNotifValidation")]
        public async Task<HttpResponseData> OrderChangeDuplicatePNRNotifValidation( [HttpTrigger( AuthorizationLevel.Anonymous, "get",Route = "OrderChangeDuplicatePNRNotif")] HttpRequestData req)
        {
            var result = new EAPI.Domain.Entities.Common.Response<string> { Status = EAPI.Domain.Entities.Common.Status.Accepted };

            return await ResultExtention<string>.ExecuteResultAsync(req, result);
        }
    }
}
