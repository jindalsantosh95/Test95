using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.PSS.Models;

namespace VAA.EAPI.Microservices.Retail.Order.Search.Application.Query
{
    public class PNRRetrieveQuery : IRequest<Result<Response<PNRCommonV6RetrieveResponse>>>
    {
        public string PNR { get; set; }
    }
}
