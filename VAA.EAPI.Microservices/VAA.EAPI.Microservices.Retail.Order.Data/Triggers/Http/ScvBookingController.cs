using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.TripDetails;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.Retail.Order.Data.Application.Query.SvcOrderStore;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.FlightBookings.Entity;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Triggers.Http
{
    public sealed class ScvBookingController(IScvBookingQuery query)
    {
        private readonly IScvBookingQuery _query = query ?? throw new ArgumentNullException(nameof(query));

        [Function("ScvFlightDateLookup")]
        [OpenApiOperation(operationId: "ScvFlightDateLookup", tags: new[] { "OrderRetrieve" }, Summary = "Returns SCV bookings matching a PNR, scheduled local flight departure date and passenger name", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody("application/json", typeof(ScvBookingLookupRequest), Required = true)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<TripDetailsResponse>), CustomHeaderType = typeof(OpenApiCustomResponseHeader), Description = "Exactly one matching booking, or an empty list for no match or ambiguity")]
        [OpenApiResponseWithBody(HttpStatusCode.BadRequest, "application/json", typeof(List<ErrorDetails>))]
        [OpenApiResponseWithBody(HttpStatusCode.Unauthorized, "application/json", typeof(List<ErrorDetails>))]
        [OpenApiResponseWithBody(HttpStatusCode.InternalServerError, "application/json", typeof(List<ErrorDetails>))]
        public async Task<HttpResponseData> LookupAsync([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "scv/orders/by-flight-date")] HttpRequestData req)
        {
            var request = await req.ParseAndValidateRequestAsync<ScvBookingLookupRequest>(bodyRequired: true);
            var result = await _query.HandleAsync(request.Pnr, request.FlightDate, request.FirstName, request.LastName, req.FunctionContext.CancellationToken);
            return await ResultExtention<IReadOnlyCollection<TripDetailsResponse>>.ExecuteResultAsync(req, result);
        }
    }
}
