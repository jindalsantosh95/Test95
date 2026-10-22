using MediatR;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Web.Common.Helper;
using VAA.EAPI.Web.Common.Infrastructure.Entities.GraphQL;
using VAA.EAPI.Web.Common.Serialisation;
using VAA.EAPI.Web.Search.Models.Common;
using VAA.EAPI.Web.Search.Models.Search.Request;
using VAA.EAPI.Web.Search.Models.Search.Response.TravelPlus;

namespace VAA.EAPI.Web.Search.Application.Query
{
    public class FareInfoRequestHandler : IRequestHandler<FareInfoRequest, FlightInformation>
    {
        private readonly IMediator _mediator;

        public FareInfoRequestHandler(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<FlightInformation> Handle(FareInfoRequest request, CancellationToken cancellationToken)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            if (ConfigReader.Get<bool>(Constants.Environment.GraphQLShowMockResponse))
            {
                return Json.Deserialize<FlightInformation>(File.ReadAllText($"./MockResponses/VAH-FareConditions.json"));
            }

            var graphVariable = new FareRulesGraphQLRequest()
            {
                OfferId = request.OfferId
            };

            var graphQLRequest = new GraphQLRequest
            {
                PqId = ConfigReader.Get(Constants.Environment.GraphQLFareConditionsPqId),
                Variables = JObject.Parse(Json.Serialize(new { request = graphVariable }, false)),
                Headers = new() { { Constants.Application.BasketIdHeaderName, request.BasketId } }
            };
            var graphQLResponse = await _mediator.Send(graphQLRequest, cancellationToken);

            var fareRulesResponse = Json.DeserializeObject<FlightInformation>(graphQLResponse.Data["fareConditions"]);
            return fareRulesResponse;
        }
    }
}
