using MediatR;
using Newtonsoft.Json.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Web.Common.Extensions;
using VAA.EAPI.Web.Common.Helper;
using VAA.EAPI.Web.Common.Infrastructure.Entities.GraphQL;
using VAA.EAPI.Web.Common.Serialisation;
using VAA.EAPI.Web.Search.Models.Search.Request;
using VAA.EAPI.Web.Search.Models.Search.Response.TravelPlus;

namespace VAA.EAPI.Web.Search.Application.Query
{

    public class BookingSessionRequestHandler : IRequestHandler<BookingSessionRequest, Booking>
    {
        private readonly IMediator _mediator;

        public BookingSessionRequestHandler(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<Booking> Handle(BookingSessionRequest request, CancellationToken cancellationToken)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            var graphQLRequest = new GraphQLRequest
            {
                PqId = ConfigReader.Get(Constants.Environment.GraphQLRetrieveBasketPqId),
                Variables = JObject.Parse(Json.Serialize(new
                {
                    basketId = request.BasketId
                }))
            };

            var graphQLResponse = await _mediator.Send(graphQLRequest, cancellationToken);

            var booking = (Booking)request;

            var summary = graphQLResponse.Data["retrieveBasket"]["summary"];

            booking.Price.Amount = summary["totalPrice"].ToString().Parse<decimal>();
            booking.Price.Currency = summary["priceCurrency"].ToString();

            return booking;
        }
    }
}
