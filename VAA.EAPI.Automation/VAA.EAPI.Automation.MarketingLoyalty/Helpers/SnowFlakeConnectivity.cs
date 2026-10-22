using System.Data;
using System.Text;
using Snowflake.Data.Client;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.MarketingLoyalty.Constants;

namespace VAA.EAPI.Automation.MarketingLoyalty.Helpers
{
    public static class SnowFlakeConnectivity
    {
        private static string connectionString;
        static SnowFlakeConnectivity()
        {
            string key = Base64Decode(Vault.Default.GetSecretValue("Snowflake-RSAKeyValue"));

            connectionString = $"account={Vault.Default.GetSecretValue("Snowflake-Account")};authenticator={Application.SnowFlakeAuthenticator};user={Application.SnowFlakeUser};" +
                $"ROLE={Application.SnowFlakeRole};private_key={key};private_key_pwd={Vault.Default.GetSecretValue("Snowflake-KeyPassword")};db={Application.SnowFlakeDataBase};" +
                $"schema={Application.SnowFlakeSchema};WAREHOUSE={Application.SnowFlakeWarehouse}";
        }

        public static string Base64Decode(string base64EncodedData)
        {
            byte[] base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public static async Task<object> SnowflakeQueryResponse(string strQuery)
        {

            object queryResponse = null;

            int retry = 5;

            using IDbConnection con = new SnowflakeDbConnection();

            con.ConnectionString = connectionString;

            if (con.State == ConnectionState.Closed)
                con.Open();

            IDbCommand cmd = con.CreateCommand();

            cmd.Connection = con;

            cmd.CommandText = strQuery;

            cmd.CommandTimeout = 120;

            IDataReader reader = cmd.ExecuteReader();

            while (retry > 0 && reader == null)
            {
                await Task.Delay(500);
                reader = cmd.ExecuteReader();
                retry--;
            }
            while (reader.Read())
            {
                queryResponse = reader.GetValue(0);
            }
            if (con.State == ConnectionState.Open)
            {
                con.Close();
            }
            return queryResponse;
        }

        public static async Task<Dictionary<string, string>> MultiRowKeyValuePairQueryResponse(string snowflakeQuery)
        {
            Dictionary<string, string> queryResponse = new();

            int retry = 5;

            using IDbConnection con = new SnowflakeDbConnection();
            con.ConnectionString = connectionString;
            if (con.State == ConnectionState.Closed)
                con.Open();

            IDbCommand cmd = con.CreateCommand();

            cmd.Connection = con;

            cmd.CommandText = snowflakeQuery;

            cmd.CommandTimeout = 120;

            IDataReader reader = cmd.ExecuteReader();

            while (retry > 0 && reader == null)
            {
                await Task.Delay(500);
                reader = cmd.ExecuteReader();
                retry--;
            }

            while (reader.Read())
            {
                if (!queryResponse.ContainsKey((string)reader.GetValue(0)))
                {
                    queryResponse.Add((string)reader.GetValue(0), (string)reader.GetValue(1));
                }
            }


            if (con.State == ConnectionState.Open)
            {
                con.Close();
            }

            return queryResponse;
        }
    }
}

