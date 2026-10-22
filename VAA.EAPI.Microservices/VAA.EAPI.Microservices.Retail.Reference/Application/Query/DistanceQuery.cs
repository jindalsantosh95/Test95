using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using CSharpFunctionalExtensions;

namespace VAA.EAPI.Microservices.Retail.Reference.Application.Query
{
    public class DistanceQuery : IRequest<Result<Response<string>>>
    {
        public string OriginCountryCode { get; set; }
        public string DestinationCountryCode { get; set; }

    }
}
