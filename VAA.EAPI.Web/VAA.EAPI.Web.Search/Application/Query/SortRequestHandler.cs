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
using VAA.EAPI.Web.Search.Models.Search.Request;
using VAA.EAPI.Web.Search.Models.Search.Response.TravelPlus;

namespace VAA.EAPI.Web.Search.Application.Query
{
    public class SortRequestHandler : IRequestHandler<SortRequest, SearchResultsResponse>
    {
        private readonly IMediator _mediator;

        public SortRequestHandler(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<SearchResultsResponse> Handle(SortRequest request, CancellationToken cancellationToken)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            var sliceData = new SliceData(request.SliceId);

            var flightOfferRequest = new FlightOfferRequest()
            {
                BasketId = request.BasketId,
                SortRequest = request.SelectedSort,
                Filters = new Models.OfferMS.FilterRequest
                {
                    Airlines = request.Filters.Airlines,
                    DepartureTimes = request.Filters.DepartureTimes,
                    Connections = request.Filters.Connections,
                    Cabins = request.Filters.Cabins
                }
            };
            if (request.OfferId.IsNullOrEmpty() == false)
            {
                flightOfferRequest.FlightSearchRequest = new()
                {
                    OfferID = request.OfferId,
                    CurrentTripIndexId = sliceData.CurrentSlice.ToString()
                };
            }

            var graphQLPQIdSearch = sliceData.CurrentSlice == 0 ? ConfigReader.Get(Constants.Environment.GraphQLSearchOffersPqId) : ConfigReader.Get(Constants.Environment.GraphQLSearchOffersNextPqId);

            var graphQLRequest = new GraphQLRequest
            {
                PqId = graphQLPQIdSearch,
                Variables = JObject.Parse(Json.Serialize(new { request = flightOfferRequest }, false)),
                Headers = GetHeaders(request.AuthToken)
            };

            return (SearchResultsResponse)Json.DeserializeObject<SearchResults>((await _mediator.Send(graphQLRequest, cancellationToken)).Data[graphQLPQIdSearch]);
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
