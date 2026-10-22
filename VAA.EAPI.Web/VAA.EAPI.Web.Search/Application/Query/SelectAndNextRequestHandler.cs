using MediatR;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Web.Common.Extensions;
using VAA.EAPI.Web.Common.Helper;
using VAA.EAPI.Web.Common.Infrastructure.Entities.GraphQL;
using VAA.EAPI.Web.Common.Serialisation;
using VAA.EAPI.Web.Search.Constants;
using VAA.EAPI.Web.Search.Models.Common;
using VAA.EAPI.Web.Search.Models.OfferMS;
using VAA.EAPI.Web.Search.Models.Search.Request;
using VAA.EAPI.Web.Search.Models.Search.Response.TravelPlus;

namespace VAA.EAPI.Web.Search.Application.Query
{
    public class SelectAndNextRequestHandler : IRequestHandler<SelectAndNextRequest, Result>
    {
        private readonly IMediator _mediator;

        public SelectAndNextRequestHandler(IMediator mediator)
        {
            _mediator = mediator ?? throw new System.ArgumentNullException(nameof(mediator));
        }

        public async Task<Result> Handle(SelectAndNextRequest request, CancellationToken cancellationToken)
        {
            var sliceData = new SliceData(request.SliceId);

            var graphQLRequest = new GraphQLRequest
            {
                PqId = ConfigReader.Get(Environment.GraphQLSearchOffersNextPqId),
                Variables = JObject.Parse(Json.Serialize(new
                {
                    request = new FlightOfferNextRequest()
                    {
                        BasketId = request.BasketId,
                        FlightSearchRequest = new()
                        {
                            OfferID = request.OfferId,
                            CurrentTripIndexId = sliceData.CurrentSlice.ToString(),
                            SearchOriginDestinations = request.slices.Select(s => new SearchOriginDestination()
                            {
                                Origin = s.origin,
                                Destination = s.destination,
                                DepartureDate = s.departure.ToString("yyyy-MM-dd"),
                                ConnectionAirports = s.connections
                            })
                        }
                    }
                }, false)),

                Headers = GetHeaders(request.AuthToken)
            };

            var graphQLResponse = await _mediator.Send(graphQLRequest, cancellationToken);

            var searchResults = Json.DeserializeObject<SearchResults>(graphQLResponse.Data[ConfigReader.Get(Environment.GraphQLSearchOffersNextPqId)]);

            return searchResults.Result;
        }

        public static Dictionary<string, string> GetHeaders(string token)
        {
            var headers = new Dictionary<string, string>()
            {
                { "PostbackUrl", ConfigReader.Get(Constants.Environment.PostbackUrl) },
            };

            if (!token.IsNullOrEmpty())
            {
                headers.Add("Authorization", $"Bearer {token}");
            }

            return headers;
        }
    }
}
