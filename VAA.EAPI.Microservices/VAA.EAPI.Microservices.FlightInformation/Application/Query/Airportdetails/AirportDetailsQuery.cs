using CSharpFunctionalExtensions;
using MediatR;
using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.Microservices.FlightInformation.Application.Query.Airportdetails
{
    public class AirportDetailsQuery : IRequest<Result<Response<List<Domain.Airportdetails.Model.Airportdetail>>>>
    {

    }
}
