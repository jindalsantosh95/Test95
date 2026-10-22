using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Services;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Repositories
{
    public class RefundAPIRepoCommandHanlder : IRequestHandler<RefundAPIRepoCommand, bool>
    {
        private readonly ILogger<RefundAPIRepoCommandHanlder> _logger;
        private readonly IMediator _mediator;

        public RefundAPIRepoCommandHanlder(IMediator mediator, ILogger<RefundAPIRepoCommandHanlder> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> Handle(RefundAPIRepoCommand request, CancellationToken cancellationToken)
        {
            return (await _mediator.Send(new RefundAPISvcCommand(), cancellationToken)).Key;
        }
    }
}
