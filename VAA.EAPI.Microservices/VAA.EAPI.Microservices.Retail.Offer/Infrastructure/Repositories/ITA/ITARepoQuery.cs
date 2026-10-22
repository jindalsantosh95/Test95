using MediatR;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Repositories.Ita
{
    public class ItaRepoQuery : IRequest<result>
    {
        public search Search { get; set; }
    }
}
