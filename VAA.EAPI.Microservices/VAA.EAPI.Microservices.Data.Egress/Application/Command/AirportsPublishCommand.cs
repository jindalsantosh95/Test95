using MediatR;
using System.Collections.Generic;
using VAA.EAPI.Microservices.Data.Egress.Models;

namespace VAA.EAPI.Microservices.Data.Egress.Application.Command
{
    public class AirportsPublishCommand : IRequest<bool>
    {
        public IEnumerable<Airport> Airports { get; set; }
    }
}
