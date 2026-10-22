using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Data;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Ndc.Validation.Application.Query.AgencyOCNDetails;
using VAA.EAPI.Microservices.Retail.Ndc.Validation.Infrastructure.Models;
using VAA.EAPI.Microservices.Retail.Ndc.Validation.Infrastructure.Services.Interfaces;

namespace VAA.EAPI.Microservices.Retail.Ndc.Validation.Infrastructure.Services
{
    public class AgencyOCNDetailsService(AgencyOCNDataContext agencyOCNDataContext) : IAgencyOCNDetailsService
    {
        private readonly AgencyOCNDataContext _agencyOCNDataContext = agencyOCNDataContext;

        public async Task<IEnumerable<AgencyOCNData>> GetAgencyOCNDetails(AgencyOCNDetailsQuery requestData)
        {
            SqlParameter[] sqlParameters =
            [
                new SqlParameter { ParameterName = "@requestAgencyId", Value = requestData.AgencyId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 15 },
                new SqlParameter { ParameterName = "@AggregatorId", Value = (object)requestData.AggregatorId ?? DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.NVarChar, Size = 15 },
                new SqlParameter { ParameterName = "@environment", Value = requestData.Environment, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 15 }
            ];

            var storedProcedure = ConfigReader.Get(Constants.Environment.NDCConnectAgencyOCNStoredProc);

            var result = await _agencyOCNDataContext.VwIdxOcnEndpoints.FromSqlRaw(storedProcedure, sqlParameters).ToListAsync();

            return result;
        }
    }
}
