using MediatR;
using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.NDC.Offer.Domain.Model.Order;
using VAA.EAPI.NDC.Offer.Domain.Model.VAA;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.SeatMS;

namespace VAA.EAPI.NDC.Offer.Infrastructure.Services.SeatMS
{
    public class SeatMSQuery :  IRequest<SeatRS>
    {
        public string OfferID { get; set; }
        public Parties Parties { get; set; }
        public Pos Pos { get; set; }
        public Context Context { get; set; }
        public IEnumerable<FlightSegment> FlightSegments { get; set; }
        public string OrderId { get; set; }
        public string ChannelId { get; set; }
        public IEnumerable<EAPI.Domain.Virgin.Models.Customer.Customer> CustomerDetails { get; set; }
    }
}
