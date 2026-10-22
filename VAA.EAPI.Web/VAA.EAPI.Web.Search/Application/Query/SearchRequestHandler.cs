using MediatR;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Web.Common.Extensions;
using VAA.EAPI.Web.Common.Helper;
using VAA.EAPI.Web.Common.Infrastructure.Entities.GraphQL;
using VAA.EAPI.Web.Common.Serialisation;
using VAA.EAPI.Web.Search.Infrastructure.Mapper.OfferMS;
using VAA.EAPI.Web.Search.Models.Search.Request;
using VAA.EAPI.Web.Search.Models.Search.Response.TravelPlus;

namespace VAA.EAPI.Web.Search.Application.Query
{
    public class SearchRequestHandler : IRequestHandler<SearchRequestV2, SearchResultsResponse>
    {
        private readonly IMediator _mediator;

        public SearchRequestHandler(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<SearchResultsResponse> Handle(SearchRequestV2 request, CancellationToken cancellationToken)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));


            // Make GraphQL request
            var flightOfferRequest = OfferMSRequestMapper.RequestMapper(request);
            var graphQLRequest = new GraphQLRequest
            {
                PqId = ConfigReader.Get(Constants.Environment.GraphQLSearchOffersPqId),
                Variables = JObject.Parse(Json.Serialize(new { request = flightOfferRequest }, false)),
                Headers = GetHeaders(request.authToken)
            };
            var searchResults = Json.DeserializeObject<SearchResults>((await _mediator.Send(graphQLRequest, cancellationToken)).Data[ConfigReader.Get(Constants.Environment.GraphQLSearchOffersPqId)]);

            return (SearchResultsResponse)searchResults;
        }

        public static Dictionary<string, string> GetHeaders(string token)
        {
            var headers = new Dictionary<string, string>()
                {
                    { "PostbackUrl", ConfigReader.Get(Constants.Environment.PostbackUrl) }
                };

            if (!token.IsNullOrEmpty())
            {
                headers.Add("Authorization", $"Bearer {token}");
            }

            return headers;
        }
    }
}
