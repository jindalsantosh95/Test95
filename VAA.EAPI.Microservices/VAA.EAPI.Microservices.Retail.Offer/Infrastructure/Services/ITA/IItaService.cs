using System.Threading.Tasks;
using System;
using System.Threading;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Services.Ita
{
    public interface IItaService
    {
        Task<result> GetOffers(search search, System.Threading.CancellationToken cancellationToken);
        Task<result> StreamOfferSolutions(search search, Func<resultBundledSolutionSummarySolution, CancellationToken, Task> solutionHandler, CancellationToken cancellationToken);
    }
}
