using CSharpFunctionalExtensions;
using MediatR;
using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.Microservices.FlightInformation.Infrasturcture.Repositories.AirportDetailsRepo
{
    public class AirportDetailsRepoCommand : IRequest<Result<Response<List<Domain.Airportdetails.Model.Airportdetail>>>>
    {
    }
}
