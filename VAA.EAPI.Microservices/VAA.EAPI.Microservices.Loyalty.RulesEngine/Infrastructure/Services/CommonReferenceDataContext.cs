#nullable disable

using Azure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Infrastructure.Models;

namespace VAA.EAPI.Microservices.Loyalty.RulesEngine.Infrastructure.Services
{
    public class CommonReferenceDataContext : DbContext
    {
        private readonly ILogger<CommonReferenceDataContext> _logger;
        readonly StringBuilder _logText = new StringBuilder();

        public CommonReferenceDataContext(DbContextOptions<CommonReferenceDataContext> options, ILogger<CommonReferenceDataContext> logger)
            : base(options)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            GetSQLConnection();
        }
        public CommonReferenceDataContext()
        {
            //added for mocking 
        }

        private void GetSQLConnection()
        {
            _logText.AppendLine($"Common Ref DB token Start");
            Microsoft.Data.SqlClient.SqlConnection Conn = (Microsoft.Data.SqlClient.SqlConnection)Database.GetDbConnection();
            string token;
            if (string.IsNullOrEmpty(Constants.Application.CommonSQLRefDBMIToken))
            {
                token = GetCommonRefDBMIToken();
                _logText.AppendLine($"Common Ref DB token not present in Cache || Created using Managed Identity");
            }
            else
            {
                JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
                JwtSecurityToken decodedValue = handler.ReadJwtToken(Constants.Application.CommonSQLRefDBMIToken);
                DateTime jwtTokenEpiryDate = decodedValue.ValidTo;
                DateTime jwtTokenEpiryDateMinusFiveMinutes = jwtTokenEpiryDate.AddMinutes(-5);
                if (jwtTokenEpiryDateMinusFiveMinutes > DateTime.UtcNow)
                {
                    _logText.AppendLine($"Common Ref DB token fetched from cache");
                    token = Constants.Application.CommonSQLRefDBMIToken;
                }
                else
                {
                    token = GetCommonRefDBMIToken();
                    _logText.AppendLine($"Common Ref DB token expired || Created using Managed Identity");
                }
            }

            Conn.AccessToken = token;
            _logText.AppendLine($"Common Ref DB token End");
            _logger.LogInformation(_logText.ToString());
        }

        private string GetCommonRefDBMIToken()
        {
            DefaultAzureCredentialOptions opt = new DefaultAzureCredentialOptions() { ExcludeSharedTokenCacheCredential = true };
            DefaultAzureCredential credential = new DefaultAzureCredential(opt);
            Azure.Core.AccessToken token = credential.GetToken(new Azure.Core.TokenRequestContext(new[] {
                ConfigReader.Get(Constants.EnvironmentSettings.DatabaseID)
            }));
            Constants.Application.CommonSQLRefDBMIToken = token.Token;

            return Constants.Application.CommonSQLRefDBMIToken;
        }

        public virtual DbSet<PartnerProfile> PartnerProfile { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PartnerProfile>().HasNoKey();
        }
    }
}
