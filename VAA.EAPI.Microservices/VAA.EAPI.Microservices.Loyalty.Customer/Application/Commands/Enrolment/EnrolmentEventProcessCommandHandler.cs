using CSharpFunctionalExtensions;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.Enrolment
{

    public class EnrolmentEventProcessCommandHandler : IRequestHandler<EnrolmentEventProcessCommand, Result<Unit>>
    {

        private readonly IMediator _mediator;


        public EnrolmentEventProcessCommandHandler(IMediator mediator)
        {

            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));

        }

        public async Task<Result<Unit>> Handle(EnrolmentEventProcessCommand request, CancellationToken cancellationToken)
        {

            EnrolmentRequestCommand memberEnrolmentRequest = new EnrolmentRequestCommand { Data = Json.Deserialize<EnrolmentRequest>(request.Data.ToString()) };
            memberEnrolmentRequest.Id = Guid.NewGuid().ToString();

            await _mediator.Send(memberEnrolmentRequest);
            return Unit.Value;

        }
    }
}
