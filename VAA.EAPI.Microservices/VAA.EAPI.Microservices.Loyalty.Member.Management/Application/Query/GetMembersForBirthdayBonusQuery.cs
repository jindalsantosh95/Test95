using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.BirthdayBonus;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.Interface;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query
{
    public class GetMembersForBirthdayBonusQuery : IRequest<IEnumerable<BirthdayBonusMemberDetails>>
    {
    }

    public class GetMembersForBirthdayBonusQueryHandler : IRequestHandler<GetMembersForBirthdayBonusQuery, IEnumerable<BirthdayBonusMemberDetails>>
    {
        private readonly IReportingDBServices _reportingDBService;
        private readonly ILogger<GetMembersForBirthdayBonusQueryHandler> _logger;
        public GetMembersForBirthdayBonusQueryHandler(IReportingDBServices reportingDBService, ILogger<GetMembersForBirthdayBonusQueryHandler> logger)
        {
            _reportingDBService = reportingDBService ?? throw new ArgumentNullException(nameof(reportingDBService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<IEnumerable<BirthdayBonusMemberDetails>> Handle(GetMembersForBirthdayBonusQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation($"Get Members For Birthday Bonus Query Handler - Start");
                DateTime birthdayDateForRun = ConfigReader.Get<bool>(Constants.Environment.IsManualRun) ?
                ConfigReader.Get<DateTime>(Constants.Environment.BirthdayDateForManualRun) : DateTime.UtcNow;
                return await _reportingDBService.GetMembersDetailsForBirthdayBonus(birthdayDateForRun);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Get Members For Birthday Bonus Query Handler failed due to Exception");
                throw;
            }
            finally
            {
                _logger.LogInformation($"Get Members For Birthday Bonus Query Handler - End");
            }
        }
    }
}
