using CSharpFunctionalExtensions;
using MediatR;
using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.Microservices.FlightInformation.Application.Command.Airportdetails
{
    public class AirportDetailsMergeEventCommand : IRequest<Result<Response<string>>>
    {
        public List<Domain.Airportdetails.Model.Airportdetail> PayLoad { get; set; }

    }
}
