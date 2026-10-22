using System.Data;
using Microsoft.Data.SqlClient;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Models;

namespace VAA.EAPI.Automation.MarketingLoyalty.Helpers
{
    public static class AzureReportingDBHelper
    {
        private static readonly string connectionString;
        private static readonly string env = Config.Environment;

        static AzureReportingDBHelper()
        {
            connectionString = env.Is("prod") ? Vault.Default.GetSecretValue("MarketingLoyalty-avapeundbReportingDbConnectionString") : env.Is("uat") ? Vault.Default.GetSecretValue("LoopsNewUAT-avaseundbReportingDbConnectionString") : Vault.Default.GetSecretValue("MarketingLoyalty-avateundbReportingDbConnectionString");
        }

        public static object RefDbQueryResponse(string query) => SqlHelper.ExecuteScalar(query, connectionString);

        public static Dictionary<string, string> MultiRowKeyValuePairQueryResponse(string query)
        {
            Dictionary<string, string> queryResponse = [];

            using SqlCommand sqlCommand = new()
            {
                Connection = SqlHelper.Connect(connectionString),
                CommandText = query,
                CommandTimeout = 120
            };

            // do not dispose of the reader as this will close the connection
            SqlDataReader reader = sqlCommand.ExecuteReader();

            if (reader.HasRows)
                while (reader.Read())
                    if (!queryResponse.ContainsKey((string)reader.GetValue(0)))
                        queryResponse.Add((string)reader.GetValue(0), (string)reader.GetValue(1));

            return queryResponse;
        }

        public static object[][] MultiColumnReptgDbQueryResponse(string query)
        {
            using SqlCommand sqlCommand = new()
            {
                Connection = SqlHelper.Connect(connectionString),
                CommandText = query,
                CommandTimeout = 120
            };

            // do not dispose of the reader as this will close the connection
            SqlDataReader dataReader = sqlCommand.ExecuteReader();

            using DataTable dataTable = new DataTable();

            dataTable.Load(dataReader);

            return dataTable.Rows.OfType<DataRow>().Select(x => x.ItemArray).ToArray();
        }

        public static IEnumerable<string> ExecuteCampaignAssociatedMembersStoredProcedure(string storedProcName, string campaignCode)
        {
            using SqlCommand sqlCommand = new(storedProcName, SqlHelper.Connect(connectionString))
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 120
            };

            sqlCommand.Parameters.AddWithValue($"@{char.ToUpper(nameof(campaignCode)[0]) + nameof(campaignCode).Substring(1)}", SqlDbType.VarChar).Value = campaignCode;

            // do not dispose of the reader as this will close the connection
            SqlDataReader reader = sqlCommand.ExecuteReader();

            if (reader.HasRows)
                while (reader.Read())
                    yield return reader.GetString(0);
        }

        public static IEnumerable<BirthdayBonusSPData> ExecuteBirthdayBonusStoredProcedure(string storedProcName, DateTime birthDateToRun)
        {
            List<BirthdayBonusSPData> eligibleMembersList = [];

            using SqlCommand sqlCommand = new(storedProcName, SqlHelper.Connect(connectionString))
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 120
            };

            sqlCommand.Parameters.AddWithValue("@BirthdayDateForRun", SqlDbType.DateTime).Value = birthDateToRun;

            // do not dispose of the reader as this will close the connection
            SqlDataReader reader = sqlCommand.ExecuteReader();

            if (reader.HasRows)
                while (reader.Read())
                    eligibleMembersList.Add(new BirthdayBonusSPData
                    {
                        MemberNumber = reader.IsDBNull(0) ? string.Empty : reader.GetString(0),
                        BirthDate = reader.IsDBNull(1) ? DateTime.MinValue : reader.GetDateTime(1),
                        TierCode = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                        StatusCode = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                        CustomerId = reader.IsDBNull(6) ? string.Empty : reader.GetString(6)
                    });

            return eligibleMembersList;
        }

        public static List<T> GetSingleColumnList<T>(string strQuery) => SqlHelper.ExecuteReaderAsList<T>(strQuery, connectionString);
    }
}
