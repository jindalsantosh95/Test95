using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Offer.Domain.FlightOffer;

namespace VAA.EAPI.Microservices.Retail.Offer.Application.Query.Delta.FlightOffer
{
    public class FlightOfferQuery : IRequest<Result<Response<FlightOfferResponse>>>
    {
        public FlightOfferRequest Request { get; set; }
        public string Type { get; set; }
    }
}
