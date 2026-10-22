using Azure.Identity;
using Microsoft.Data.SqlClient;
using System;
using System.IdentityModel.Tokens.Jwt;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Helper
{
    public class ConnectionHelper
    {
        public static void GetSQLConnection(SqlConnection Conn)
        {
            string token;
            if (string.IsNullOrEmpty(Constants.Application.CommonSQLRefDBMIToken))
            {
                token = GetCommonRefDBMIToken();
            }
            else
            {
                JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
                JwtSecurityToken decodedValue = handler.ReadJwtToken(Constants.Application.CommonSQLRefDBMIToken);
                DateTime jwtTokenEpiryDate = decodedValue.ValidTo;
                DateTime jwtTokenEpiryDateMinusFiveMinutes = jwtTokenEpiryDate.AddMinutes(-5);
                if (jwtTokenEpiryDateMinusFiveMinutes > DateTime.UtcNow)
                {
                    token = Constants.Application.CommonSQLRefDBMIToken;
                }
                else
                {
                    token = GetCommonRefDBMIToken();
                }
            }

            Conn.AccessToken = token;
        }
        private static string GetCommonRefDBMIToken()
        {
            DefaultAzureCredentialOptions opt = new DefaultAzureCredentialOptions() { ExcludeSharedTokenCacheCredential = true };
            DefaultAzureCredential credential = new DefaultAzureCredential(opt);
            Azure.Core.AccessToken token = credential.GetToken(new Azure.Core.TokenRequestContext(new[] {
                ConfigReader.Get(Constants.Environment.DatabaseID)
            }));
            Constants.Application.CommonSQLRefDBMIToken = token.Token;

            return Constants.Application.CommonSQLRefDBMIToken;
        }
    }
}
