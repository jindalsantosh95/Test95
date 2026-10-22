using MediatR;
using Newtonsoft.Json.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Web.Common.Infrastructure.Entities.GraphQL;
using VAA.EAPI.Web.Common.Serialisation;
using VAA.EAPI.Web.Search.Models.Search.Request;
using VAA.EAPI.Web.Search.Models.Search.Response.TravelPlusSummary;

namespace VAA.EAPI.Web.Search.Application.Query
{
    public class SummaryDeeplinkRequestHandler : IRequestHandler<SummaryDeeplinkRequest, Summary>
    {
        private readonly IMediator _mediator;

        public SummaryDeeplinkRequestHandler(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<Summary> Handle(SummaryDeeplinkRequest request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var graphQLRequest = new GraphQLRequest
            {
                PqId = "SearchSummaryDeepLink",
                Variables = JObject.Parse(Json.Serialize(new
                {
                    request
                }, false))
            };

            var graphQLResponse = await _mediator.Send(graphQLRequest, cancellationToken);

            return Json.DeserializeObject<Summary>(graphQLResponse.Data["searchSummaryDeepLink"]);
        }
    }
}
