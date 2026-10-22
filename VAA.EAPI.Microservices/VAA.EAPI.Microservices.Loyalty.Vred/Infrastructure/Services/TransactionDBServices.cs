using Microsoft.Extensions.Logging;
using System;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Services.Interface;
using SqlParameter = Microsoft.Data.SqlClient.SqlParameter;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Services
{
    public class TransactionDBServices : ITransactionDBServices
    {
        private readonly TransactionDataContext _transactionDataContext;

        public TransactionDBServices(TransactionDataContext transactionDataContext)
        {
            _transactionDataContext = transactionDataContext ?? throw new ArgumentNullException(nameof(transactionDataContext));
        }

        public async Task<bool> AddTransactionAuditInfo(string membershipNumber, string transactionID, string status, string transactionType, string activityDate, string transactionProcessedDate, string transactionSource, string serviceRequest, string siebelRequest, string siebelResponse, string createdBy, string createdByEmailId, string createdByUserName)
        {

            string sql = Constants.Application.InsertTransactionAuditQuery;

            List<SqlParameter> sqlParameters = new List<SqlParameter>
                {
                    new SqlParameter { ParameterName = "@MembershipNumber", Value = membershipNumber ?? (object) DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 50 },
                    new SqlParameter { ParameterName = "@TransactionID", Value = transactionID ?? (object) DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 50 },
                    new SqlParameter { ParameterName = "@Status", Value = status ?? (object) DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 100 },
                    new SqlParameter { ParameterName = "@TransactionType", Value = transactionType ?? (object) DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 100},
                    new SqlParameter { ParameterName = "@ActivityDate", Value = activityDate ?? (object) DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 100},
                    new SqlParameter { ParameterName = "@TransactionProcessedDate", Value = transactionProcessedDate ?? (object) DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 100},
                    new SqlParameter { ParameterName = "@TransactionSource", Value = transactionSource ?? (object) DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 100},
                    new SqlParameter { ParameterName = "@ServiceRequest", Value = serviceRequest ?? (object) DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = int.MaxValue},
                    new SqlParameter { ParameterName = "@SiebelRequest", Value = siebelRequest ?? (object) DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 100},
                    new SqlParameter { ParameterName = "@SiebelResponse", Value = siebelResponse ?? (object) DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 100},
                    new SqlParameter { ParameterName = "@CreatedBy", Value = createdBy ?? (object) DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 100},
                    new SqlParameter { ParameterName = "@CreatedByEmailId", Value = createdByEmailId ?? (object) DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 100},
                    new SqlParameter { ParameterName = "@CreatedByUserName", Value = createdByUserName ?? (object) DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 100},

                    new SqlParameter { ParameterName = "@Result", Direction = ParameterDirection.Output, SqlDbType = SqlDbType.Int }
                 };

            await _transactionDataContext.Database.ExecuteSqlRawAsync(sql, sqlParameters.ToArray());

            return (int)sqlParameters[13].Value == 1 ? true : false;

        }

        public async Task<bool> UpdateVRedResponseAuditInfo(string transactionID, string vRedTransactionResponse)
        {

            string sql = Constants.Application.UpdateTransactionAuditReponseQuery;

            List<SqlParameter> sqlParameters = new List<SqlParameter>
            {

                new SqlParameter { ParameterName = "@TransactionID", Value = transactionID ?? (object) DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 50 },
                new SqlParameter { ParameterName = "@VredResponse", Value = vRedTransactionResponse ?? (object) DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = int.MaxValue },

                new SqlParameter { ParameterName = "@Result", Direction = ParameterDirection.Output, SqlDbType = SqlDbType.Int }
            };

            await _transactionDataContext.Database.ExecuteSqlRawAsync(sql, sqlParameters.ToArray());

            return (int)sqlParameters[2].Value == 1 ? true : false;

        }

        public async Task<bool> UpdateVRedRequestAuditInfo(string transactionID, string vRedTransactionRequest)
        {

            string sql = Constants.Application.UpdateTransactionAuditRequestQuery;

            List<SqlParameter> sqlParameters = new List<SqlParameter>
            {

                new SqlParameter { ParameterName = "@TransactionID", Value = transactionID ?? (object) DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 50 },
                new SqlParameter { ParameterName = "@VredRequest", Value = vRedTransactionRequest ?? (object) DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = int.MaxValue },

                new SqlParameter { ParameterName = "@Result", Direction = ParameterDirection.Output, SqlDbType = SqlDbType.Int }
            };

            await _transactionDataContext.Database.ExecuteSqlRawAsync(sql, sqlParameters.ToArray());

            return (int)sqlParameters[2].Value == 1 ? true : false;

        }
    }
}
