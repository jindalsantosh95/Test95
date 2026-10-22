using CSharpFunctionalExtensions;
using MediatR;
using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Configuration.Domain;

namespace VAA.EAPI.Microservices.Configuration.Application
{
    public class UpsellOffersDBQuery : IRequest<IEnumerable<UpsellOffersData>>
    {
    }
}
