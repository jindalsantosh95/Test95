using MediatR;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Web.Common.Helper;
using VAA.EAPI.Web.Common.Infrastructure.Entities.GraphQL;
using VAA.EAPI.Web.Common.Serialisation;
using VAA.EAPI.Web.Search.Models.CarbonIndex;

namespace VAA.EAPI.Web.Search.Application.Query
{
    public class CarbonIndexRequestHandler : IRequestHandler<Models.Search.Request.CarbonIndexRequest, IEnumerable<CarbonIndexResponse>>
    {
        private readonly IMediator _mediator;

        public CarbonIndexRequestHandler(IMediator mediator)
        {
            _mediator = mediator ?? throw new System.ArgumentNullException(nameof(mediator));
        }

        public async Task<IEnumerable<CarbonIndexResponse>> Handle(Models.Search.Request.CarbonIndexRequest request, CancellationToken cancellationToken)
        {
            var graphQLRequest = new GraphQLRequest
            {
                PqId = ConfigReader.Get(Constants.Environment.GraphQLCarbonIndexPqId),
                Variables = JObject.Parse(Json.Serialize(new { request = request }, false))
            };

            var graphQLResponse = await _mediator.Send(graphQLRequest, cancellationToken);

            return Json.DeserializeObject<IEnumerable<CarbonIndexResponse>>(graphQLResponse.Data["CarbonIndex"]);
        }
    }
}
