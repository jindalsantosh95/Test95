using Microsoft.Data.SqlClient;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Models.VglcTransactionDB;

namespace VAA.EAPI.Automation.MarketingLoyalty.Helpers
{
    public static class AzureVglcTransactionDBHelper
    {
        private static SqlConnection _connection;
        private static readonly object _lock = new();

        private static string env = Config.Environment;

        public static SqlConnection GetSqlConnection()
        {
            if (_connection == null)
            {
                lock (_lock)
                {
                    _connection = new SqlConnection(env.Is("prod") ? Vault.Default.GetSecretValue("MarketingLoyalty-avapeunsqlvglcConnectionString") :
                        Vault.Default.GetSecretValue("MarketingLoyalty-avateunsqldbvglctransactionConnectionString"));
                    _connection.Open();
                }
            }

            return _connection;
        }

        public static IEnumerable<PowerAppTransactionAuditInfo> PowerAppTransactionAuditInfo(string memberNumber)
        {
            List<PowerAppTransactionAuditInfo> powerAppTransactionAudit = new();

            SqlCommand cmd = new()
            {
                Connection = GetSqlConnection(),

                CommandText = $"SELECT top (2) * FROM [VGLCTXN].[PowerAppTransactionAuditInfo] where MembershipNumber = '{memberNumber}' order by Created desc "
            };

            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        powerAppTransactionAudit.Add
                            (
                            new PowerAppTransactionAuditInfo
                            {
                                AuditId = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                                MembershipNumber = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                                TransactionID = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                                TransactionType = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                                ActivityDate = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                                Status = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                                TransactionProcessedDate = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                                TransactionSource = reader.IsDBNull(7) ? string.Empty : reader.GetString(7),
                                ServiceRequest = reader.IsDBNull(8) ? string.Empty : reader.GetString(8),
                                SiebelRequest = reader.IsDBNull(9) ? null : reader.GetValue(9),
                                SiebelResponse = reader.IsDBNull(10) ? null : reader.GetValue(10),
                                VredRequest = reader.IsDBNull(11) ? string.Empty : reader.GetString(11),
                                VredResponse = reader.IsDBNull(12) ? string.Empty : reader.GetString(12),
                                Created = reader.IsDBNull(13) ? (DateTime?)null : reader.GetDateTime(13),
                                CreatedBy = reader.IsDBNull(14) ? string.Empty : reader.GetString(14),
                                CreatedByEmailId = reader.IsDBNull(15) ? string.Empty : reader.GetString(15),
                                CreatedByUserName = reader.IsDBNull(16) ? string.Empty : reader.GetString(16),

                            }
                            );
                    }
                }
            }
            return powerAppTransactionAudit;
        }

    }
}
