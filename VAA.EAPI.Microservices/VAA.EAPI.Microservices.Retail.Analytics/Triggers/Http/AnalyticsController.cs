using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Services.Analytics;
using VAA.EAPI.Microservices.Retail.Analytics.Application.Query;

namespace VAA.EAPI.Microservices.Retail.Analytics.Triggers.Http
{
    public sealed class AnalyticsController
    {
        private readonly IMediator _mediator;

        public AnalyticsController(IMediator mediator) => _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));

        [OpenApiOperation(operationId: "track", tags: new[] { "Track" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(bool), Summary = "OK", Description = "OK")]
        [Function("Track")]
        public async Task<HttpResponseData> Track([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "track")] HttpRequestData req)
        {
            var content = await new StreamReader(req.Body).ReadToEndAsync();

            var trackAnalyticsEventCommand = (TrackAnalyticsEventCommand)content;

            TrackAnalyticsMessage.Initialise(trackAnalyticsEventCommand);

            return await ResultExtention<bool>.ExecuteResultAsync(req, CSharpFunctionalExtensions.Result.Success(new Response<bool>()
            {
                Status = Status.OK,
                Value = await _mediator.Send(trackAnalyticsEventCommand, CancellationToken.None),
            }));
        }


        [OpenApiOperation(operationId: "feed", tags: new[] { "Feed" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Summary = "OK", Description = "OK")]
        [Function("Feed")]
        public async Task<HttpResponseData> Feed([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "feed")] HttpRequestData req)
        {
            string from = DateTime.UtcNow.ToString("yyyy-MM-dd");
            string to = from;

            if (req.Query.AllKeys.Any(k => k.Is("from") && RegexCompiled.IsMatch(req.Query["from"], @"\d{4}-\d{2}-\d{2}")))
                from = req.Query["from"];

            if (req.Query.AllKeys.Any(k => k.Is("to") && RegexCompiled.IsMatch(req.Query["to"], @"\d{4}-\d{2}-\D{2}")))
                to = req.Query["to"];

            var getDataQuery = new GetDataQuery
            {
                From = from,
                To = to,
            };

            return await ResultExtention<IEnumerable<TrackAnalyticsMessage>>.ExecuteResultAsync(req, CSharpFunctionalExtensions.Result.Success(new Response<IEnumerable<TrackAnalyticsMessage>>()
            {
                Status = Status.OK,
                Value = await _mediator.Send(getDataQuery, CancellationToken.None),
            }));
        }
    }
}