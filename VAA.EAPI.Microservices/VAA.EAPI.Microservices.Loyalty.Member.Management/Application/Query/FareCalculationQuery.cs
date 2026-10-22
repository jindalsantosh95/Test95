using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.FarePointCalc;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query
{
    public class FareCalculationQuery : IRequest<Result<Response<FareCalculationResponse>>>
    {
        public FareCalculationRequest FareCalculationRequest { get; set; }
    }
}
