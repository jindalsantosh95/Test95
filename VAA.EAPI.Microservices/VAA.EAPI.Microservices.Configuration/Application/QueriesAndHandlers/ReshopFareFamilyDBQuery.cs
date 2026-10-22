using CSharpFunctionalExtensions;
using MediatR;
using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Configuration.Domain;

namespace VAA.EAPI.Microservices.Configuration.Application.QueriesAndHandlers
{
    public class ReshopFareFamilyDBQuery : IRequest<Result<Response<IEnumerable<Reshop_FareFamily>>>>
    {

    }
}
