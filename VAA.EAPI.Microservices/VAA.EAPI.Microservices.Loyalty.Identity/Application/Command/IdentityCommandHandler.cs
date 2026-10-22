using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Repositories;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Application.Command
{
    public class IdentityCommandHandler : IRequestHandler<IdentityEventProcessCommand, Result<bool>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<IdentityCommandHandler> _logger;
        public IdentityCommandHandler(IMediator mediator, ILogger<IdentityCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<bool>> Handle(IdentityEventProcessCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var identityRequest = Json.Deserialize<CreateCustomerEntities>(Convert.ToString(request.Data));
                _logger.LogInformation($"CorrelationId: {request.Id} & CustomerId: {identityRequest.CustomerId}");
                if (identityRequest.EnrollmentChannel == Constants.Application.VredChannelID || identityRequest.EnrollmentChannel == Constants.Application.PowerAppChannelID || identityRequest.EnrollmentChannel == Constants.Application.SNAPPChannelID || identityRequest.EnrollmentChannel == Constants.Application.AxisChannelID || identityRequest.EnrollmentChannel == Constants.Application.PDMChannelID)
                {                   
                    await _mediator.Send(new IdentityRepoCommand(identityRequest));                }
                else
                {
                    _logger.LogInformation($"Cannot enrol customer in Identity for channel : {identityRequest.EnrollmentChannel}");
                }
                return true;
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Identity B2C enrolment processing failed.");
                return false;
            }
        }
    }
}
