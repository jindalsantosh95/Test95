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
using VAA.EAPI.NDC.Offer.Application.Query.ServiceList;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.ServiceListRQ;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.ServiceListRS;

namespace VAA.EAPI.NDC.Offer.Triggers.Http
{
    public class ServiceListController
    {
        private readonly IMediator _mediator;

        public ServiceListController(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [OpenApiOperation(operationId: "ServiceList", tags: ["ServiceList"], Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [Function("ServiceList")]
        public async Task<HttpResponseData> ServiceList([HttpTrigger(AuthorizationLevel.Anonymous, "POST", Route = "ServiceList")] HttpRequestData httpRQ)
        {
            var serviceListRQ = Common.Serialisation.Xml.Deserialize<IATA_ServiceListRQ>(await new StreamReader(httpRQ.Body).ReadToEndAsync());

            var subscriptionHeaders = Enumerable.Empty<string>();
            httpRQ.Headers?.TryGetValues(Constants.Application.ApimSubscription, out subscriptionHeaders);
            var subscriptionKey = subscriptionHeaders.FirstOrDefault();

            var result = await _mediator.Send(new ServiceListQuery(serviceListRQ, subscriptionKey));

            return await ResultExtention<IATA_ServiceListRS>.ExecuteResultAsync(httpRQ, result, true);
        }
    }
}