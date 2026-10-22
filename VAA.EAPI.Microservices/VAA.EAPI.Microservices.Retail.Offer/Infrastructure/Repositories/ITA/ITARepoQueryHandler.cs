using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Services.Ita;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Repositories.Ita
{
    public class ItaRepoQueryHandler : IRequestHandler<ItaRepoQuery, result>
    {
        private readonly IItaService _itaService;

        public ItaRepoQueryHandler(IItaService iTaService)
        {
            _itaService = iTaService ?? throw new ArgumentNullException(nameof(iTaService));
        }

        public async Task<result> Handle(ItaRepoQuery request, CancellationToken cancellationToken)
        {
            return await _itaService.GetOffers(request.Search, cancellationToken);
        }
    }
}
