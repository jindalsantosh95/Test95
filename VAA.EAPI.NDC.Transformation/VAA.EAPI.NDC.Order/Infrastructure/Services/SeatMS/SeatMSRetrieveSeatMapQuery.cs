using MediatR;
using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.NDC.Order.Infrastructure.Model.SeatMS;
using Pos = VAA.EAPI.Domain.Virgin.Models.Common.Pos;

namespace VAA.EAPI.NDC.Order.Infrastructure.Services.SeatMS
{
    public class SeatMSRetrieveSeatMapQuery : IRequest<SeatRS>
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
