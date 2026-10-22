
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Models;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Services.Interface;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Services
{
    public class ReportingDBServices : IReportingDBServices
    {
        private readonly ReportingDataContext _reportingDataContext;
        private readonly ILogger<ReportingDBServices> _logger;

        public ReportingDBServices(ReportingDataContext reportingDataContext, ILogger<ReportingDBServices> logger)
        {
            _reportingDataContext = reportingDataContext ?? throw new ArgumentNullException(nameof(reportingDataContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<int> GetAssociatedMemberCount(string campaignCode)
        {
            try
            {
                List<SqlParameter> sqlParameters = new()
                {
                    new() { ParameterName = "@CampaignCode", Value = (object)campaignCode ?? DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.NVarChar, Size = 60 },
                    new() { ParameterName = "@MemberCount", Direction = ParameterDirection.Output, SqlDbType = SqlDbType.Int }
                };

                await _reportingDataContext.Database.ExecuteSqlRawAsync(Constants.Application.GetAssociatedMemCountQuery, sqlParameters);

                return (int)sqlParameters[1].Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAssociatedMemberCount failed due to Exception");
                throw;
            }
        }

        public async Task<List<AssociatedMemberData>> GetAssociatedMembersData(string campaignCode)
        {
            try
            {
                List<SqlParameter> sqlParameters = new List<SqlParameter>
                {
                    new() { ParameterName = "@CampaignCode", Value = (object)campaignCode ?? DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.NVarChar, Size = 60 }
                };

                var a = await _reportingDataContext.AssociatedMemberData.FromSqlRaw(Constants.Application.GetAssociatedMembers, sqlParameters.ToArray()).ToListAsync();

                return a;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAssociatedMembers failed due to Exception");
                throw;
            }
        }
    }
}
