using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.PSS.Proxy.PNRCommonV6Proxy;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderSearch.Infrastructure.Repositories
{
    public class PNRRetrieveQuery : IRequest<Result<Response<RetrieveResponseType>>>
    {
        public string PNR { get; set; }
    }
}
