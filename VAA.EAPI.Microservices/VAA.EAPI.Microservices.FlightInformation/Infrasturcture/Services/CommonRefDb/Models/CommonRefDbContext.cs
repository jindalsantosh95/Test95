using Azure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.IdentityModel.Tokens.Jwt;
using VAA.EAPI.Common.Helpers;

#nullable disable

namespace VAA.EAPI.Microservices.FlightInformation.Infrasturcture.Services.CommonRefDb.Models
{
    public class CommonRefDbContext : DbContext
    {
        public CommonRefDbContext()
        {
        }

        public CommonRefDbContext(DbContextOptions<CommonRefDbContext> options, ILogger<CommonRefDbContext> logger)
            : base(options)
        {
            GetSQLConnection();
        }
        private void GetSQLConnection()
        {
            Microsoft.Data.SqlClient.SqlConnection Conn = (Microsoft.Data.SqlClient.SqlConnection)Database.GetDbConnection();
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

        private string GetCommonRefDBMIToken()
        {
            DefaultAzureCredentialOptions opt = new DefaultAzureCredentialOptions() { ExcludeSharedTokenCacheCredential = true };
            DefaultAzureCredential credential = new DefaultAzureCredential(opt);
            Azure.Core.AccessToken token = credential.GetToken(new Azure.Core.TokenRequestContext(new[] {
                ConfigReader.Get(Constants.Environment.DatabaseID)
            }));
            Constants.Application.CommonSQLRefDBMIToken = token.Token;

            return Constants.Application.CommonSQLRefDBMIToken;
        }

        public virtual DbSet<DimAirport> DimAirports { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

            modelBuilder.Entity<DimAirport>(entity =>
            {
                entity.HasKey(e => e.AirportID)
                    .HasName("PK__tmp_ms_x__3DBD86135D8A023C");

                entity.ToTable("Dim_Airport", "DM");

                entity.Property(e => e.AirportID).HasColumnName("Airport_ID");

                entity.Property(e => e.CityIataCode)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("City_IATA_Code");

                entity.Property(e => e.Country)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CountryCode)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("Country_Code");

                entity.Property(e => e.EffectiveEndDate)
                    .HasColumnType("date")
                    .HasColumnName("Effective_End_Date");

                entity.Property(e => e.EffectiveStartDate)
                    .HasColumnType("date")
                    .HasColumnName("Effective_Start_Date");

                entity.Property(e => e.IataAirportCode)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("Iata_Airport_Code");

                entity.Property(e => e.Name)
                    .HasMaxLength(150)
                    .IsUnicode(false);

            });
            OnModelCreatingPartial(modelBuilder);
        }

        public virtual void OnModelCreatingPartial(ModelBuilder modelBuilder) { }
    }
}
