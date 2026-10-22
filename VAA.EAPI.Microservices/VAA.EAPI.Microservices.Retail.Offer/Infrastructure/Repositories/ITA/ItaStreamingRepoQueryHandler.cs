using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Services.Ita;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Repositories.Ita
{
    public class ItaStreamingRepoQueryHandler : IRequestHandler<ItaStreamingRepoQuery, result>
    {
        private readonly IItaService _itaService;

        public ItaStreamingRepoQueryHandler(IItaService itaService)
        {
            _itaService = itaService ?? throw new ArgumentNullException(nameof(itaService));
        }

        public Task<result> Handle(ItaStreamingRepoQuery request, CancellationToken cancellationToken) =>
            _itaService.StreamOfferSolutions(request.Search, request.SolutionHandler, cancellationToken);
    }
}
