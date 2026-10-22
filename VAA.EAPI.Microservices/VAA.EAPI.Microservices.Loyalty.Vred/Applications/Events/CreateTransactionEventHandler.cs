using System;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.Vred.Validators;
using VAA.EAPI.Microservices.Loyalty.Vred.Applications.ErrorHandling;
using VAA.EAPI.Microservices.Loyalty.Vred.Applications.Commands.Transaction;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Applications.Events
{
    public class CreateTransactionEventHandler : IRequestHandler<CreateTransactionEvent, Unit>
    {
        private readonly IMediator _mediator;

        public CreateTransactionEventHandler(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<Unit> Handle(CreateTransactionEvent eventRequest, CancellationToken cancellationToken)
        {
            CreateTransactionRequestCommand command = Json.Deserialize<CreateTransactionRequestCommand>(Convert.ToString(eventRequest.Data));
           
            var validationResult = new CreateTransactionRequestCommandValidator().Validate(command);
           
            if (!validationResult.IsValid)
                throw new VRedException("Invalid Event.", validationResult.Errors);

            await _mediator.Send(new CreateTransactionCommand() { Id = eventRequest.Id, Request = command.Request });
          
            return Unit.Value;
        }
    }
}