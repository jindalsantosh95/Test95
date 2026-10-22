using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.NDC.Offer.Application.Query.SeatAvailability;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.SeatAvailabilityRQ;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.SeatAvailabilityRS;

namespace VAA.EAPI.NDC.Offer.Triggers.Http
{
    public class SeatAvailabilityController
    {
        private readonly IMediator _mediator;

        public SeatAvailabilityController(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [OpenApiOperation(operationId: "SeatAvailability", tags: ["SeatAvailability"], Summary = "Fetch the seatmap for VS segments", Description = "Check the segment details and fetch all cabin seatmap for particular VS flight", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [Function("SeatAvailability")]
        public async Task<HttpResponseData> SeatAvailability([HttpTrigger(AuthorizationLevel.Anonymous, "POST", Route = "SeatAvailability")] HttpRequestData httpRQ)
        {
            var seatAvailabilityRQ = Common.Serialisation.Xml.Deserialize<IATA_SeatAvailabilityRQ>(await new StreamReader(httpRQ.Body).ReadToEndAsync());

            var subscriptionHeaders = Enumerable.Empty<string>();
            httpRQ.Headers?.TryGetValues(Constants.Application.ApimSubscription, out subscriptionHeaders);
            var subscriptionKey = subscriptionHeaders.FirstOrDefault();

            var result = await _mediator.Send(new SeatAvailabilityQuery(seatAvailabilityRQ, subscriptionKey));

            return await ResultExtention<IATA_SeatAvailabilityRS>.ExecuteResultAsync(httpRQ, result, true);
        }
    }
}