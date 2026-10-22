using MediatR;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Web.Book.Helper;
using VAA.EAPI.Web.Book.Models.YourDetails;
using VAA.EAPI.Web.Book.Models.YourDetails.Response;
using VAA.EAPI.Web.Common.Helper;
using VAA.EAPI.Web.Common.Infrastructure.Entities.GraphQL;
using VAA.EAPI.Web.Common.Serialisation;

namespace VAA.EAPI.Web.Book.Infrastructure.Repositories.GraphQl
{
    public class RetrieveBasketGraphQLHandler : IRequestHandler<RetrieveBasketGraphQLQuery, YourDetailsResponse>
    {
        private readonly IMediator _mediator;

        public RetrieveBasketGraphQLHandler(IMediator mediator)
        {
            _mediator = mediator ?? throw new System.ArgumentNullException(nameof(mediator));
        }

        public async Task<YourDetailsResponse> Handle(RetrieveBasketGraphQLQuery retrieveBasketGraphQLQuery, CancellationToken cancellationToken)
        {
            var basketGqlRequest = new { request = new RetrieveBasketRequest(retrieveBasketGraphQLQuery.BasketId, retrieveBasketGraphQLQuery.AuthToken) };

            var basketGraphQLRequest = new GraphQLRequest
            {
                PqId = ConfigReader.Get(Constants.Environment.RetrieveBasketPqId),
                Variables = JObject.Parse(Json.Serialize(basketGqlRequest, false)),
                Headers = Utility.GetHeaders(retrieveBasketGraphQLQuery.AuthToken)
            };

            var basketGraphQLResponse = await _mediator.Send(basketGraphQLRequest, cancellationToken);

            return Json.DeserializeObject<YourDetailsResponse>(basketGraphQLResponse.Data["retrieveBasket"]);
        }
    }
}
