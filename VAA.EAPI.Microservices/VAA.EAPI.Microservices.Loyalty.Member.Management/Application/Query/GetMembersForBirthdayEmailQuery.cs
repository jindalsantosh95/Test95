using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.BirthdayEmail;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.Interface;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query
{
    public class GetMembersForBirthdayEmailQuery : IRequest<IEnumerable<MemberContactData>>
    {
    }

    public class GetMembersForBirthdayEmailQueryHandler : IRequestHandler<GetMembersForBirthdayEmailQuery, IEnumerable<MemberContactData>>
    {
        private readonly IReportingDBServices _reportingDBService;
        private readonly ILogger<GetMembersForBirthdayEmailQueryHandler> _logger;
        public GetMembersForBirthdayEmailQueryHandler(IReportingDBServices reportingDBService, ILogger<GetMembersForBirthdayEmailQueryHandler> logger)
        {
            _reportingDBService = reportingDBService ?? throw new ArgumentNullException(nameof(reportingDBService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<IEnumerable<MemberContactData>> Handle(GetMembersForBirthdayEmailQuery request, CancellationToken cancellationToken)
        {
            IEnumerable<MemberContactData> memberList = null;
            try
            {
                _logger.LogInformation($"Get Members For Birthday Email Query Handler - Start");
                memberList = await _reportingDBService.GetMembersBirthdayEmailDetails();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Get Members For Birthday Email Query Handler failed due to Exception");
                throw;
            }
            finally
            {
                _logger.LogInformation($"Get Members For Birthday Email Query Handler - End");
            }
            return memberList;
        }
    }
}
