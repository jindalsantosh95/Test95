using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.CustomerAssignAward;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.CustomerAssignAward;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.CustomerAssignAward
{
    public class CustomerAssignAwardEventProcessCommandHandler : IRequestHandler<CustomerAssignAwardEventProcessCommand, bool>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CustomerAssignAwardEventProcessCommandHandler> _logger;

        public CustomerAssignAwardEventProcessCommandHandler(IMediator mediator, ILogger<CustomerAssignAwardEventProcessCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> Handle(CustomerAssignAwardEventProcessCommand request, CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            try
            {
                var data = Json.Deserialize<AssignAwardRequestData>(Convert.ToString(request.Data));
                await _mediator.Send(new CustomerAssignAwardRepoCommand(data, request.Id));
                return true;
            }

            catch (Exception ex)
            {
                _logger.LogInformation($"Exception occured - {ex}");
            }

            return false;
        }
    }
}
