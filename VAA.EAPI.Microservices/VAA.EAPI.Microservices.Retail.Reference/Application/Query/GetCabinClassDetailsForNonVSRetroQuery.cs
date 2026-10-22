using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Reference.Domain;

namespace VAA.EAPI.Microservices.Retail.Reference.Application.Query
{
    public class GetCabinClassDetailsForNonVSRetroQuery : NonVSRetroFlightDetails, IRequest<Result<Response<NonVSRetroCabinClassDetails>>>
    {
    }
}
