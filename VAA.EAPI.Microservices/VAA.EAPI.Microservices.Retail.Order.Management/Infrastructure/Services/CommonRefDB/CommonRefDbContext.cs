#nullable disable

using Azure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Model;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Services.CommonRefDB
{
    public class CommonRefDbContext : DbContext
    {
        private readonly ILogger<CommonRefDbContext> _logger;
        readonly StringBuilder _logText = new StringBuilder();

        public CommonRefDbContext(DbContextOptions<CommonRefDbContext> options, ILogger<CommonRefDbContext> logger)
            : base(options)
        {
            _logger = logger;
            GetSQLConnection();
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
            _logger.LogDebug(_logText.ToString());
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


        public virtual DbSet<DimCarrier> DimCarriers { get; set; }
        public virtual DbSet<DimAirport> DimAirports { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

            modelBuilder.Entity<DimCarrier>(entity =>
            {
                entity.HasKey(e => e.CarrierId)
                    .HasName("PK__Dim_Carr__6E53225A78634FB8");

                entity.ToTable("Dim_Carrier", "DM");

                entity.Property(e => e.CarrierId).HasColumnName("Carrier_ID");

                entity.Property(e => e.AirlineName)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("Airline_Name");

                entity.Property(e => e.BaseAirport)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("Base_Airport");

                entity.Property(e => e.CarrierAlliance)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("Carrier_Alliance");

                entity.Property(e => e.CarrierGrouping)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("Carrier_Grouping");

                entity.Property(e => e.CarrierType)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("Carrier_Type");

                entity.Property(e => e.Country)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CurrentFlag).HasColumnName("Current_Flag");

                entity.Property(e => e.EffectiveEndDate)
                    .HasColumnType("date")
                    .HasColumnName("Effective_End_Date");

                entity.Property(e => e.EffectiveStartDate)
                    .HasColumnType("date")
                    .HasColumnName("Effective_Start_Date");

                entity.Property(e => e.IataAccountingCode)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("IATA_Accounting_Code");

                entity.Property(e => e.IataAlphaCode)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("IATA_Alpha_Code");

                entity.Property(e => e.IcaoCode)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("ICAO_Code");

                entity.Property(e => e.InferredFlag).HasColumnName("Inferred_Flag");
            });
            modelBuilder.Entity<DimAirport>(entity =>
            {
                entity.HasKey(e => e.AirportId)
                    .HasName("PK__tmp_ms_x__3DBD86135D8A023C");

                entity.ToTable("Dim_Airport", "DM");

                entity.HasIndex(e => e.IataAirportCode, "uix_Dim_Airport_IATA_Airport_Code")
                    .IsUnique();

                entity.Property(e => e.AirportId).HasColumnName("Airport_ID");

                entity.Property(e => e.AirportAltitudeFt).HasColumnName("Airport_Altitude_Ft");

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

                entity.Property(e => e.CurrentFlag).HasColumnName("Current_Flag");

                entity.Property(e => e.DecimalLatitude)
                    .HasColumnType("decimal(18, 5)")
                    .HasColumnName("Decimal_Latitude");

                entity.Property(e => e.DecimalLongitude)
                    .HasColumnType("decimal(18, 5)")
                    .HasColumnName("Decimal_Longitude");

                entity.Property(e => e.DstOffset)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("DST_Offset");

                entity.Property(e => e.EffectiveEndDate)
                    .HasColumnType("date")
                    .HasColumnName("Effective_End_Date");

                entity.Property(e => e.EffectiveStartDate)
                    .HasColumnType("date")
                    .HasColumnName("Effective_Start_Date");

                entity.Property(e => e.FromUtcOffset)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("From_UTC_Offset");

                entity.Property(e => e.GlobalRegion)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("Global_Region");

                entity.Property(e => e.IataAirportCode)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("Iata_Airport_Code");

                entity.Property(e => e.IcaoAirportCode)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("ICAO_Airport_Code");

                entity.Property(e => e.InferredFlag).HasColumnName("Inferred_Flag");

                entity.Property(e => e.IsAfklHub).HasColumnName("Is_AFKL_Hub");

                entity.Property(e => e.IsDlHub).HasColumnName("Is_DL_Hub");

                entity.Property(e => e.IsVsHub).HasColumnName("Is_VS_Hub");

                entity.Property(e => e.Latitude)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Longitude)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.RouteSwareAptNme)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("Route_SWARE_APT_NME");

                entity.Property(e => e.State)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.StateName)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("State_Name");

                entity.Property(e => e.ToUtcDtsOffset)
                    .HasMaxLength(11)
                    .IsUnicode(false)
                    .HasColumnName("To_UTC_DTS_Offset");

                entity.Property(e => e.ToUtcOffset)
                    .HasMaxLength(11)
                    .IsUnicode(false)
                    .HasColumnName("To_UTC_Offset");

                entity.Property(e => e.Wac).HasColumnName("WAC");

                entity.Property(e => e.WacName)
                    .HasMaxLength(150)
                    .HasColumnName("WAC_Name");

                entity.Property(e => e.WikiLink)
                    .HasMaxLength(150)
                    .HasColumnName("Wiki_Link");
            });
            OnModelCreatingPartial(modelBuilder);
        }

        public virtual void OnModelCreatingPartial(ModelBuilder modelBuilder) { }
    }
}
