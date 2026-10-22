using CSharpFunctionalExtensions;
using MediatR;
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
using VAA.EAPI.Common;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.NDC.Order.Application.OrderChangeNotif;
using VAA.EAPI.NDC.Order.Domain;

namespace VAA.EAPI.NDC.Order.Triggers.Http
{
    public class OrderChangeNotifController(IMediator mediator, ILogger<OrderChangeNotifController> logger, IOrderChangeNotifCommand orderChangeNotifCommand, ITtlExpiryCommand ttlExpiryCommand)
    {
        private readonly IMediator _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        private readonly ILogger<OrderChangeNotifController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IOrderChangeNotifCommand _orderChangeNotifCommand = orderChangeNotifCommand ?? throw new ArgumentNullException(nameof(orderChangeNotifCommand));
        private readonly ITtlExpiryCommand _ttlExpiryCommand = ttlExpiryCommand ?? throw new ArgumentNullException(nameof(ttlExpiryCommand));

        [OpenApiOperation(operationId: "OrderChangeNotif", tags: ["OrderChangeNotif"], Summary = "This endpoint is used to send order change notifications to agent", Description = "This endpoint is used to send order change notifications to agent", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody("application/xml", typeof(OrderChangeNotifRQ), Description = "")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Accepted, CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "Request accepted", Description = "Request accepted")]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [Function("OrderChangeNotif")]
        public async Task<HttpResponseData> OrderChangeNotif([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "OrderChangeNotif")] HttpRequestData req)
        {
            //Temporary logging to see the request coming from Sabre ENS - to be removed 
            var request = await new StreamReader(req.Body).ReadToEndAsync();
            _logger.LogInformation($"OCN message - {request}");

            XDocument xml = XDocument.Parse(request);

            var body = xml.Descendants().First(e => e.Name.LocalName == "Body").Elements().First();

            // instead of wrapping the contents of handle in  _ = Task.Run(async () => { ... }),
            // we can just call the handle method and not await it, since we don't need to wait
            // for it to complete before returning the response. This will allow the function
            // to return a response immediately while the handle method continues to run in the
            // background.
            var handleTask = _orderChangeNotifCommand.Handle(Xml.Deserialize<OrderChangeNotifRQ>(body.ToString()), System.Threading.CancellationToken.None);

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

        [OpenApiOperation(operationId: "OrderChangeNotifValidation", tags: ["OrderChangeNotifValidation"], Summary = "This endpoint is used by sabre for validation", Description = "This endpoint is used by sabre for validation", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Accepted, CustomHeaderType = typeof(OpenApiCustomResponseHeader), Summary = "Request accepted", Description = "Request accepted")]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [Function("OrderChangeNotifValidation")]
        public async Task<HttpResponseData> OrderChangeNotifValidation([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "OrderChangeNotif")] HttpRequestData req)
        {
            //This endpoint always returns 202 success
            var result = new EAPI.Domain.Entities.Common.Response<string>() { Status = EAPI.Domain.Entities.Common.Status.Accepted };

            return await ResultExtention<string>.ExecuteResultAsync(req, result);
        }

        [Function("OrderChangeNotifTtlExpirySubscriber")]
        public async Task OrderChangeNotifTtlExpiry(
            [ServiceBusTrigger(Constants.Application.TtlExpiryTopicName, Constants.Application.TtlExpirySubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload,
            System.Threading.CancellationToken cancellationToken)
        {
            var cloudEvent = Json.Deserialize<CloudEvent>(eventRequestPayload);

            var request = Json.Deserialize<OcnTtlExpiryRQ>(Json.Serialize(cloudEvent.Data));

            AsyncContext.Current.MapFromCloudEvent(cloudEvent);

            await _ttlExpiryCommand.Handle(request, cloudEvent.Subject, cancellationToken);
        }
    }
}
