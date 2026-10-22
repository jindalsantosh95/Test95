using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.ChangeFeedProcessor.Infrasturcture.Repository;

namespace VAA.EAPI.Microservices.Loyalty.ChangeFeedProcessor.Application.Command
{
    public class ChangeFeedProcesCommandHandler : IRequestHandler<ChangeFeedProcesCommand, bool>
    {
        private readonly ILogger<ChangeFeedProcesCommandHandler> _logger;
        private readonly IMediator _mediator;
        public ChangeFeedProcesCommandHandler(IMediator mediator, ILogger<ChangeFeedProcesCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<bool> Handle(ChangeFeedProcesCommand request, CancellationToken cancellationToken)
        {
            return await _mediator.Send(new CustomerSearchRepoCommand(request.CustomerSearch));
        }
    }
}
