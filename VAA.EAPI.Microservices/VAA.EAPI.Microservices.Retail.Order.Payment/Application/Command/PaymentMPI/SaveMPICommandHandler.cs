using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Repositories;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Application.Command.PaymentMPI
{
    public class SaveMPICommandHandler : IRequestHandler<SaveMPICommand, Result<Response<string>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<SaveMPICommandHandler> _logger;

        public SaveMPICommandHandler(IMediator mediator, ILogger<SaveMPICommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<string>>> Handle(SaveMPICommand request, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(new SaveMPIRepoCommand(request.AuthenticationRequest), cancellationToken);

            return new Response<string> { Status = Status.OK, Value = response.Value.Value };

        }
    }
}
