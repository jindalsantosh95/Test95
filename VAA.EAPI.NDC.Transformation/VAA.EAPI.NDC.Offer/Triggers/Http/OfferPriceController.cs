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
using VAA.EAPI.NDC.Offer.Application.Query.OfferPrice;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.OfferPriceRQ;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.OfferPriceRS;

namespace VAA.EAPI.NDC.Offer.Triggers.Http
{
    public class OfferPriceController
    {
        private readonly IMediator _mediator;

        public OfferPriceController(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [OpenApiOperation(operationId: "OfferPrice", tags: ["OfferPrice"], Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [Function("OfferPrice")]
        public async Task<HttpResponseData> OfferPrice([HttpTrigger(AuthorizationLevel.Anonymous, "POST", Route = "OfferPrice")] HttpRequestData httpRQ)
        {
            var offerPriceRQ = Common.Serialisation.Xml.Deserialize<IATA_OfferPriceRQ>(await new StreamReader(httpRQ.Body).ReadToEndAsync());

            var subscriptionHeaders = Enumerable.Empty<string>();
            httpRQ.Headers?.TryGetValues(Constants.Application.ApimSubscription, out subscriptionHeaders);
            var subscriptionKey = subscriptionHeaders.FirstOrDefault();

            var result = await _mediator.Send(new OfferPriceQuery(offerPriceRQ, subscriptionKey));

            return await ResultExtention<IATA_OfferPriceRS>.ExecuteResultAsync(httpRQ, result, true);
        }
    }
}
