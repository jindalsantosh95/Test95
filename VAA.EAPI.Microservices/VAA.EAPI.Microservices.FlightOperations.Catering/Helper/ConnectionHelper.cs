using Azure.Identity;
using Microsoft.Data.SqlClient;
using System;
using System.IdentityModel.Tokens.Jwt;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Microservices.FlightOperations.Catering.Helper
{
    public static class ConnectionHelper
    {
        private static string popsDBMIToken;

        public static void GetSQLConnection(SqlConnection Conn)
        {
            string token;
            if (popsDBMIToken.IsJwtTokenValid())
            {
                token = popsDBMIToken;
            }
            else
            {
                token = GetPopsDBMIToken();
            }
                     
            Conn.AccessToken = token;
        }

        private static string GetPopsDBMIToken()
        {
            DefaultAzureCredentialOptions opt = new DefaultAzureCredentialOptions() { ExcludeSharedTokenCacheCredential = true };
            DefaultAzureCredential credential = new DefaultAzureCredential(opt);
            Azure.Core.AccessToken token = credential.GetToken(new Azure.Core.TokenRequestContext(new[] {
                ConfigReader.Get(Constants.Environment.DatabaseID)
            }));
            popsDBMIToken = token.Token;

            return popsDBMIToken;
        }
    }
}
