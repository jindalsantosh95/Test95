using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Models.B2CGraph;
using VAA.EAPI.Microservices.Retail.Order.Infrastructure.Services.InflightOrders;

namespace VAA.EAPI.Microservices.Retail.Order.Infrastructure.Repositories.InflightOrders
{
    public class B2CGraphRepoHandler : IRequestHandler<B2CGraphRepoQuery, B2CGraphResponse>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<B2CGraphRepoHandler> _logger;
        private readonly StringBuilder _logText = new StringBuilder();

        public B2CGraphRepoHandler(IMediator mediator, ILogger<B2CGraphRepoHandler> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }
        public async Task<B2CGraphResponse> Handle(B2CGraphRepoQuery request, CancellationToken cancellationToken)
        {
            _logText.AppendLine("Customer Orders Get Loyalty No Repo handler");
            var response = await _mediator.Send(new B2CGraphSvcQuery(request?.CustomerId), cancellationToken);

            if (!response.Key)
            {
                _logText.AppendLine("Customer Orders Get Loyalty Number Business exception");
                _logger.LogInformation(_logText.ToString());
                throw new Exception(response.Value);
            }

            return Json.Deserialize<B2CGraphResponse>(response.Value);
        }
    }
}
