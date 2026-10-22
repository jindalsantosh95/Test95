using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Offer.Domain.RewardSeatAvailability;
using VAA.EAPI.Microservices.Retail.Offer.Interfaces;

namespace VAA.EAPI.Microservices.Retail.Offer.Triggers.Http.Ita
{

    public sealed class RewardSeatCalendarController
    {
        private readonly IRewardSeatCalendarService _service;
        private readonly ILogger<RewardSeatCalendarController> _logger;

        public RewardSeatCalendarController(IRewardSeatCalendarService service, ILogger<RewardSeatCalendarController> logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [Function("RewardSeatCalendarAvailability")]
        [OpenApiOperation(operationId: "RewardSeatCalendarAvailability", tags: ["Reward availability"], Summary = "Retrieve normalized reward calendar availability", Description = "Anonymous availability lookup; no user login, Entra token, application role, or client ID allowlist is required.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody("application/json", typeof(RewardSeatCalendarRequest))]
        [OpenApiParameter(name: "x-reward-seat-deadline-ms", In = ParameterLocation.Header, Required = true, Type = typeof(int), Description = "Remaining downstream execution budget in milliseconds. Supply exactly one integer from 1 to 30000 inclusive. Example: 25000. Missing, repeated, malformed or out-of-range values return HTTP 400.")]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(RewardSeatCalendarResponse))]
        [OpenApiResponseWithBody(HttpStatusCode.BadRequest, "application/json", typeof(List<ErrorDetails>))]
        [OpenApiResponseWithBody(HttpStatusCode.BadGateway, "application/json", typeof(List<ErrorDetails>))]
        public async Task<HttpResponseData> SearchAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "reward-seat-calendar-availability")]
            HttpRequestData request,
            CancellationToken cancellationToken)
        {
            if (!request.Headers.TryGetValues("x-reward-seat-deadline-ms", out var values) ||
                System.Linq.Enumerable.Take(values, 2).ToArray() is not [{ } value] ||
                !int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var milliseconds) ||
                milliseconds is < 1 or > 30000)
                return await ErrorResponseAsync(request, Constants.ErrorCodes.RewardSeatInvalidDeadline, Status.BadRequest);

            RewardSeatCalendarRequest input;
            try
            {
                using var reader = new StreamReader(request.Body);
                input = Json.Deserialize<RewardSeatCalendarRequest>(await reader.ReadToEndAsync(cancellationToken));
            }
            catch (Exception error) when (error is Newtonsoft.Json.JsonException or ArgumentException)
            {
                return await ErrorResponseAsync(request, Constants.ErrorCodes.RewardSeatInvalidRequest, Status.BadRequest);
            }

            try
            {
                var result = await _service.SearchAsync(input, TimeSpan.FromMilliseconds(milliseconds), cancellationToken);
                return await ResultExtention<RewardSeatCalendarResponse>.ExecuteResultAsync(request,
                    new Response<RewardSeatCalendarResponse> { Status = Status.OK, Value = result });
            }
            catch (ArgumentException)
            {
                return await ErrorResponseAsync(request, Constants.ErrorCodes.RewardSeatInvalidRequest, Status.BadRequest);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception error)
            {
                _logger.LogError(error, "Reward seat calendar availability failed.");
                var response = await ErrorResponseAsync(request, Constants.ErrorCodes.RewardSeatUnavailable, Status.Error);
                // The shared status enum has no BadGateway member; preserve this endpoint's 502 contract.
                response.StatusCode = HttpStatusCode.BadGateway;
                return response;
            }
        }

        private static Task<HttpResponseData> ErrorResponseAsync(HttpRequestData request, string code, Status status) =>
            ResultExtention<RewardSeatCalendarResponse>.ExecuteResultAsync(request,
                new Response<RewardSeatCalendarResponse>
                {
                    Status = status,
                    Errors = ErrorMapper.MapTechnicalExceptionToEAPIException(code, Guid.NewGuid().ToString(),
                        VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)
                });
    }
}
