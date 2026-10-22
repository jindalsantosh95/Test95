using MediatR;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Web.Common.Extensions;
using VAA.EAPI.Web.Common.Helper;
using VAA.EAPI.Web.Common.Infrastructure.Entities.GraphQL;
using VAA.EAPI.Web.Common.Serialisation;
using VAA.EAPI.Web.Search.Models.Common;
using VAA.EAPI.Web.Search.Models.GraphQl;
using VAA.EAPI.Web.Search.Models.OfferMS;
using VAA.EAPI.Web.Search.Models.Search.Request;
using VAA.EAPI.Web.Search.Models.Search.Response.TravelPlusSummary;

namespace VAA.EAPI.Web.Search.Application.Query
{
    public class SelectAndSummaryRequestHandler : IRequestHandler<SelectAndSummaryRequest, Summary>
    {
        private readonly IMediator _mediator;

        public SelectAndSummaryRequestHandler(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<Summary> Handle(SelectAndSummaryRequest request, CancellationToken cancellationToken)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            var graphQLRequest = new GraphQLRequest
            {
                PqId = "SearchSummary",
                Variables = JObject.Parse(Json.Serialize(new
                {
                    request = new SearchSummaryRequest()
                    {
                        OfferId = request.OfferId,
                        BasketId = request.BasketId,
                        SummaryLink = request.SummaryLink
                    }
                }, false)),

                Headers = GetHeaders(request.AuthToken)
            };

            var graphQLResponse = await _mediator.Send(graphQLRequest, cancellationToken);

            return Json.DeserializeObject<Summary>(graphQLResponse.Data["searchSummary"]);
        }

        public static Dictionary<string, string> GetHeaders(string token)
        {
            var headers = new Dictionary<string, string>();

            if (!token.IsNullOrEmpty())
            {
                headers.Add("Authorization", $"Bearer {token}");
            }

            return headers;
        }
    }
}
