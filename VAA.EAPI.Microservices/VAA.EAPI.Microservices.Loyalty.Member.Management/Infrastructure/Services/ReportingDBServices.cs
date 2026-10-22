using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.BirthdayBonus;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.BirthdayEmail;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.PromotionEnrolledMembers;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.WingsGoldMemberAssessment;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.Interface;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services
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

        public async Task<IEnumerable<MemberContactData>> GetMembersBirthdayEmailDetails()
        {
            try
            {
                var memberDetails = await _reportingDataContext.MemberEmailDetails.FromSqlRaw(Constants.Application.GetMembersForBirthdayEmail).ToListAsync();
                _logger.LogInformation($"ReportingDBServices GetMembersBirthdayEmailDetails - End");
                return memberDetails;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"ReportingDBServices GetMembersBirthdayEmailDetails failed with exception");
                throw;
            }
        }

        public async Task<IEnumerable<BirthdayBonusMemberDetails>> GetMembersDetailsForBirthdayBonus(DateTime birthdayDateForRun)
        {
            try
            {
                string sql = Constants.Application.GetMembersForBirthdayBonus;
                SqlParameter[] sqlParameters = new SqlParameter[Constants.Application.ParamCount]
                {
                    new SqlParameter { ParameterName = "@BirthdayDateForRun", Value = birthdayDateForRun, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.DateTime },
                };
                var memberDetails = await _reportingDataContext.BirthdayBonusMemberDetails.FromSqlRaw(sql, sqlParameters.ToArray()).ToListAsync();
                _logger.LogInformation($"ReportingDBServices GetMembersDetailsForBirthdayBonus - End");
                return memberDetails;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"ReportingDBServices GetMembersDetailsForBirthdayBonus failed with exception");
                throw;
            }
        }

        public async Task<WingsAssessmentStagingResult> StageGoldMemberDetailsForWingsAssessment()
        {
            try
            {
                var productDescriptionParameter = new SqlParameter
                {
                    ParameterName = "@ProductDescription",
                    Value = Constants.Application.AwardType,
                    Direction = ParameterDirection.Input,
                    SqlDbType = SqlDbType.NVarChar,
                    Size = 255
                };
                var isSuccessfulParameter = new SqlParameter
                {
                    ParameterName = "@IsSuccessful",
                    Direction = ParameterDirection.Output,
                    SqlDbType = SqlDbType.Bit
                };
                var statusMessageParameter = new SqlParameter
                {
                    ParameterName = "@StatusMessage",
                    Direction = ParameterDirection.Output,
                    SqlDbType = SqlDbType.NVarChar,
                    Size = 4000
                };
                var rowsInsertedParameter = new SqlParameter
                {
                    ParameterName = "@RowsInserted",
                    Direction = ParameterDirection.Output,
                    SqlDbType = SqlDbType.Int
                };

                await _reportingDataContext.Database.ExecuteSqlRawAsync(
                    Constants.Application.StageGoldMemberDetailsForWingsAssessment,
                    productDescriptionParameter,
                    isSuccessfulParameter,
                    statusMessageParameter,
                    rowsInsertedParameter);

                var stagingResult = new WingsAssessmentStagingResult(
                    isSuccessfulParameter.Value is true,
                    statusMessageParameter.Value?.ToString(),
                    rowsInsertedParameter.Value is int rowsInserted ? rowsInserted : 0);

                _logger.LogInformation(
                    "ReportingDBServices staged {RowsInserted} Wings assessment members. Succeeded: {Succeeded}",
                    stagingResult.RowsInserted,
                    stagingResult.IsSuccessful);

                return stagingResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ReportingDBServices StageGoldMemberDetailsForWingsAssessment failed with exception");
                throw;
            }
        }

        public async Task<WingsAssessmentMemberBatch> GetWingsMemberAssessmentBatch(int memberCount)
        {
            if (memberCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(memberCount), "Member count must be greater than zero.");
            }

            try
            {
                var memberCountParameter = new SqlParameter
                {
                    ParameterName = "@memCount",
                    Value = memberCount,
                    Direction = ParameterDirection.Input,
                    SqlDbType = SqlDbType.Int
                };
                var hasMoreUnprocessedRecordsParameter = new SqlParameter
                {
                    ParameterName = "@HasMoreUnprocessedRecords",
                    Direction = ParameterDirection.Output,
                    SqlDbType = SqlDbType.Bit
                };
                var memberDetails = await _reportingDataContext.GoldMemberDetails
                    .FromSqlRaw(
                        Constants.Application.GetWingsMemberAssessmentBatch,
                        memberCountParameter,
                        hasMoreUnprocessedRecordsParameter)
                    .ToListAsync();
                var memberBatch = new WingsAssessmentMemberBatch(
                    memberDetails,
                    hasMoreUnprocessedRecordsParameter.Value is true);

                _logger.LogInformation(
                    "ReportingDBServices retrieved {MemberCount} Wings assessment members. Has more unprocessed members: {HasMoreUnprocessedRecords}",
                    memberBatch.Members.Count,
                    memberBatch.HasMoreUnprocessedRecords);

                return memberBatch;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ReportingDBServices GetWingsMemberAssessmentBatch failed with exception");
                throw;
            }
        }

        public async Task<IEnumerable<PromotionEnrolledMembers>> GetEnrolledMemberDetails(string promotionId)
        {
            try
            {
                string sql = Constants.Application.GetPromotionEnrolledMembers;

                List<SqlParameter> sqlParameters = new List<SqlParameter>
                {
                new SqlParameter { ParameterName = "@PromotionId", Value = promotionId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 60 },
                };

                var memberDetails = _reportingDataContext.EnrolledMemberDetails.FromSqlRaw(sql, sqlParameters.ToArray()).ToList();

                if (memberDetails.Any())
                {
                    _logger.LogInformation($"ReportingDBServices GetEnrolledMemberDetails - End");
                    return memberDetails;
                }
                else
                {
                    _logger.LogInformation("Fecth Enrolled Members from Reporting DB is Failed");
                    return new List<PromotionEnrolledMembers>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"ReportingDBServices GetEnrolledMemberDetails failed with exception");
                throw;
            }
        }

        public async Task<string> DeleteTargetedMembership(string customerId, string documentId)
        {
            try
            {
                string sql = Constants.Application.DeleteTargetedMembership;
                List<SqlParameter> sqlParameters = new List<SqlParameter>
                {
                    new SqlParameter { ParameterName = "@campaignId", Value = documentId ?? (object) DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 60 },
                    new SqlParameter { ParameterName = "@customerId", Value = customerId ?? (object) DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 50 },
                    new SqlParameter { ParameterName = "@Result", Direction = ParameterDirection.Output, SqlDbType = SqlDbType.Int }
                 };
                await _reportingDataContext.Database.ExecuteSqlRawAsync(sql, sqlParameters.ToArray());
                return (int)sqlParameters.LastOrDefault().Value == 1 ? Constants.Application.Success : (int)sqlParameters.LastOrDefault().Value == 0 ? Constants.Application.TargetedMembershipNotFound : Constants.Application.Faliure;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"ReportingDBServices DeleteTargetedMembership Process  failed with exception");
                throw;
            }
        }
    }
}

