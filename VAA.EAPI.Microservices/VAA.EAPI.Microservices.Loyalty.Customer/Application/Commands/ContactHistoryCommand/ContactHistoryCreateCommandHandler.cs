using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.ContactHistoryDocument;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.ContactHistoryCommand
{
    public class ContactHistoryCreateCommandHandler : IRequestHandler<ContactHistoryCreateCommand, Result<Unit>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ContactHistoryCreateCommandHandler> _logger;

        public ContactHistoryCreateCommandHandler(IMediator mediator, ILogger<ContactHistoryCreateCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Unit>> Handle(ContactHistoryCreateCommand request, CancellationToken cancellationToken)
        {
            try
            {
                await _mediator.Send(new ContactHistoryRepoCreateCommand(request.ContactHistoryEntities), cancellationToken);
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error Occured while creating Contact History Document: {ex.Message}");
            }

            return Unit.Value;
        }
    }
}
