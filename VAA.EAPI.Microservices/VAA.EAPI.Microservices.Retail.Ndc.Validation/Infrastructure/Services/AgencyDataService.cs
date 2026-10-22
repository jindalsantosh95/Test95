using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Microservices.Retail.Ndc.Validation.Application.Query.AgencyValidate;
using VAA.EAPI.Microservices.Retail.Ndc.Validation.Infrastructure.Models;
using VAA.EAPI.Microservices.Retail.Ndc.Validation.Infrastructure.Services.Interfaces;
using SqlParameter = Microsoft.Data.SqlClient.SqlParameter;

namespace VAA.EAPI.Microservices.Retail.Ndc.Validation.Infrastructure.Services
{
    public class AgencyDataService : IAgencyDataService
    {
        private readonly AgencyDataContext _agencyDataContext;
        private readonly ILogger<AgencyDataService> _logger;

        public AgencyDataService(AgencyDataContext agencyDataContext, ILogger<AgencyDataService> logger)
        {
            _agencyDataContext = agencyDataContext;
            _logger = logger;
        }

        public async Task<List<AgencyData>> GetAgencyDetails(AgencyValidateQuery requestData)
        {
            SqlParameter[] sqlParameters = new[]
            {
                new SqlParameter { ParameterName = "@requestAgencyId", Value = (object)requestData.Parties.Participants.FirstOrDefault(p => p.PartyType == Participant.PartyTypeEnum.Seller)?.AgencyID ?? DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 15 },
                new SqlParameter { ParameterName = "@requestAgencyType", Value =  string.Empty , Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 50},
                new SqlParameter { ParameterName = "@environment", Value = string.IsNullOrEmpty(requestData.Environment) ? DBNull.Value : requestData.Environment, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 15 },
                new SqlParameter { ParameterName = "@keyType", Value =  DBNull.Value , Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 50},
                new SqlParameter { ParameterName = "@corporateId", Value = string.IsNullOrEmpty(requestData.Parties?.CorporateAccount?.CorporateCode) ? string.Empty : requestData.Parties.CorporateAccount.CorporateCode, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 20},
            };

            var storedProcedure = ConfigReader.Get(Constants.Environment.NDCConnectStoredProc);

            var result = await _agencyDataContext.VwIdxKeyValidations.FromSqlRaw(storedProcedure, sqlParameters).ToListAsync();

            return result;
        }
    }
}
