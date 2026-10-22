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
using VAA.EAPI.NDC.Offer.Application.Query.AirShopping;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.AirShoppingRQ;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.AirShoppingRS;

namespace VAA.EAPI.NDC.Offer.Triggers.Http
{
    public class AirShoppingController
    {
        private readonly IMediator _mediator;

        public AirShoppingController(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [OpenApiOperation(operationId: "AirShopping", tags: ["AirShopping"], Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [Function("AirShopping")]
        public async Task<HttpResponseData> AirShopping([HttpTrigger(AuthorizationLevel.Anonymous, "POST", Route = "AirShopping")] HttpRequestData httpRQ)
        {
            var airShoppingRQ = Common.Serialisation.Xml.Deserialize<IATA_AirShoppingRQ>(await new StreamReader(httpRQ.Body).ReadToEndAsync());

            var subscriptionHeaders = Enumerable.Empty<string>();
            httpRQ.Headers?.TryGetValues(Constants.Application.ApimSubscription, out subscriptionHeaders);
            var subscriptionKey = subscriptionHeaders.FirstOrDefault();

            var result = await _mediator.Send(new AirShoppingQuery(airShoppingRQ, subscriptionKey));

            return await ResultExtention<IATA_AirShoppingRS>.ExecuteResultAsync(httpRQ, result, true);
        }
    }
}