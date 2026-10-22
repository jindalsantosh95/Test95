using MediatR;
using System;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;
using VAA.EAPI.Infrastructure.PSS.Models;

namespace VAA.EAPI.Infrastructure.PSS.Repository.Query
{
    public class FlightSchedulesAvailabilityQuery : IRequest<Segment>
    {
        public Air4RequestContext Air4RequestContext { get; set; }
        public string Carrier { get; set; }
        public string FlightNumber { get; set; }
        public DateTime FlightDate { get; set; }
        public string Origin { get; set; }
        public string Destination { get; set; }
        public string SessionId { get; set; }
    }
}