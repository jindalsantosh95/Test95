using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Reference.Domain;

namespace VAA.EAPI.Microservices.Retail.Reference.Application.Query
{
    public class DistanceAndCountryCodeQuery : IRequest<Result<Response<DistanceAndCountryCodeResponse>>>
    {
        public string OriginAirportCode { get; set; }
        public string DestinationAirportCode { get; set; }
    }
}
