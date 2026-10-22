using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enums;
using VAA.EAPI.Microservices.Loyalty.Customer.Mapper.CustomerAssignAward;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.CustomerAssignAward
{
    public class LoyaltyCustomerAssignAwardCommandHandler : IRequestHandler<LoyaltyCustomerAssignAwardCommand, Result<Response<string>>>
    {
        private readonly ILogger<LoyaltyCustomerAssignAwardCommandHandler> _logger;
        private readonly IMediator _mediator;


        public LoyaltyCustomerAssignAwardCommandHandler(IMediator mediator, ILogger<LoyaltyCustomerAssignAwardCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<string>>> Handle(LoyaltyCustomerAssignAwardCommand request, CancellationToken cancellationToken)
        {

            if (request == null) throw new ArgumentNullException(nameof(request));

            var id = Guid.NewGuid().ToString();

            try
            {
                var cloudEvent = AssignAwardCommandToEventMapper.CommandToEventMapper(request);

                id = cloudEvent.Id;

                await _mediator.Send(new EventRequestInsertCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent, Subject = cloudEvent.Subject }, cancellationToken);
                await _mediator.Publish(cloudEvent, cancellationToken);
                return Result.Success(new Response<string> { Status = Status.Accepted });
            }

            catch (Exception ex)
            {
                await _mediator.Send(new EventStoreRequestDeleteCommand { Id = id, Type = CustomerEventType.AssignLoyaltyAward.ToString(), Data = request, Subject = Constants.Application.AssignSwardEventSubject }, cancellationToken);
                return Result.Failure<Response<string>>(Convert.ToString(ex.Message));
            }
        }
    }
}
