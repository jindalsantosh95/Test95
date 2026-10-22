using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.Interface;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Application.Command.CampaignUnassign
{
    public class TargetedMembershipReportingCommandHandler : IRequestHandler<TargetedMembershipReportingCommand, Result<Response<string>>>
    {
        private readonly IReportingDBServices _reportingDBServices;
        private readonly ILogger<TargetedMembershipReportingCommandHandler> _logger;

        public TargetedMembershipReportingCommandHandler(IReportingDBServices reportingDBServices, ILogger<TargetedMembershipReportingCommandHandler> logger)
        {
            _reportingDBServices = reportingDBServices ?? throw new ArgumentNullException(nameof(reportingDBServices));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<string>>> Handle(TargetedMembershipReportingCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation($"Targeted Membership delete Reporting DB Command Handler - Start");
                string deleteResponse = await _reportingDBServices.DeleteTargetedMembership(request.CustomerId, request.DocumentId);
                return new Response<string>
                {
                    Value = deleteResponse
                };
            }
            finally
            {
                _logger.LogInformation($"Targeted Membership delete Reporting DB Command Handler - End");
            }
        }

    }
}
