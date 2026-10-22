using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Repositories.Ita
{
    public class ItaStreamingRepoQuery : IRequest<result>
    {
        public search Search { get; set; }
        public Func<resultBundledSolutionSummarySolution, CancellationToken, Task> SolutionHandler { get; set; }
    }
}
