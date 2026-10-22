using Azure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.IdentityModel.Tokens.Jwt;
using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Microservices.Retail.Order.Search.Infrastructure.Repositories.SCVReferenceDB.Model
{
    public class SCVReferenceDBContext : DbContext
    {
        private readonly ILogger<SCVReferenceDBContext> _logger;

        public SCVReferenceDBContext()
        {
        }

        public SCVReferenceDBContext(DbContextOptions<SCVReferenceDBContext> options, ILogger<SCVReferenceDBContext> logger)
            : base(options)
        {
            _logger = logger;
            GetSQLConnection();
        }

        private void GetSQLConnection()
        {
            Microsoft.Data.SqlClient.SqlConnection Conn = (Microsoft.Data.SqlClient.SqlConnection)Database.GetDbConnection();
            string token;

            if (string.IsNullOrEmpty(Constants.Application.SQLRefDBMIToken))
            {
                token = GetCommonRefDBMIToken();
            }
            else
            {
                JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
                JwtSecurityToken decodedValue = handler.ReadJwtToken(Constants.Application.SQLRefDBMIToken);
                DateTime jwtTokenEpiryDate = decodedValue.ValidTo;
                DateTime jwtTokenEpiryDateMinusFiveMinutes = jwtTokenEpiryDate.AddMinutes(-5);

                if (jwtTokenEpiryDateMinusFiveMinutes > DateTime.UtcNow)
                {
                    token = Constants.Application.SQLRefDBMIToken;
                }
                else
                {
                    token = GetCommonRefDBMIToken();
                }

            }

            Conn.AccessToken = token;
        }

        private string GetCommonRefDBMIToken()
        {
            DefaultAzureCredentialOptions opt = new DefaultAzureCredentialOptions() { ExcludeSharedTokenCacheCredential = true };
            DefaultAzureCredential credential = new DefaultAzureCredential(opt);
            try
            {
                Azure.Core.AccessToken token = credential.GetToken(new Azure.Core.TokenRequestContext(new[] {
                    ConfigReader.Get(Constants.Environment.DatabaseID)
                }));
                Constants.Application.SQLRefDBMIToken = token.Token;
                _logger.LogInformation("SCV Reference DB managed-identity access token acquired or refreshed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SCV Reference DB managed-identity access token acquisition failed");
                throw;
            }

            return Constants.Application.SQLRefDBMIToken;
        }

        public virtual DbSet<Orders> Orders { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Orders>().HasNoKey();
        }

        public virtual void OnModelCreatingPartial(ModelBuilder modelBuilder) { }
    }
}
