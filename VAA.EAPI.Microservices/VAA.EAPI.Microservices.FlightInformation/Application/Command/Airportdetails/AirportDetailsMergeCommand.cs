using MediatR;
using System.Collections.Generic;

namespace VAA.EAPI.Microservices.FlightInformation.Application.Command.Airportdetails
{
    public class AirportDetailsMergeCommand : IRequest<List<Domain.Airportdetails.Model.Airportdetail>>
    {
        public List<Domain.Airportdetails.Model.Airportdetail> AirportDetails { get; set; }
    }
}
