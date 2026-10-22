using MediatR;
using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;
using VAA.EAPI.Microservices.Retail.Seat.Domain;
using VAA.EAPI.Microservices.Retail.Seat.Infrastructure.Proxy.SeatV8Proxy;

namespace VAA.EAPI.Microservices.Retail.Seat.Infrastructure.Repositories.SeatAvailability
{
    public class SeatV8RetrievePassengerMapQuery : IRequest<RetrievePassengerMapResponseType>
    {
        public string RecordLocator { get; set; }
        public string CurrencyCode { get; set; }
        public IEnumerable<Segment> FlightSegments { get; set; }
        public Channel ChannelInfo { get; set; }
        public IEnumerable<Customer> Customers { get; set; }
    }
}
