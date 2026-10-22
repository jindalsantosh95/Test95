using Azure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.IdentityModel.Tokens.Jwt;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Reference.Domain;

namespace VAA.EAPI.Microservices.Retail.Reference.Infrastructure.Repositories.SCVReferenceDB.Models
{
    public class ScvReferenceDbContext : DbContext
    {
        public ScvReferenceDbContext()
        {
        }

        public ScvReferenceDbContext(DbContextOptions<ScvReferenceDbContext> options, ILogger<ScvReferenceDbContext> logger)
            : base(options)
        {
            GetSQLConnection();
        }

        private void GetSQLConnection()
        {
            Microsoft.Data.SqlClient.SqlConnection Conn = (Microsoft.Data.SqlClient.SqlConnection)Database.GetDbConnection();

            string token;

            if (string.IsNullOrEmpty(Constants.Application.SQLRefDbMIToken))
            {
                token = GetCommonRefDBMIToken();
            }
            else
            {
                JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
                JwtSecurityToken decodedValue = handler.ReadJwtToken(Constants.Application.SQLRefDbMIToken);
                DateTime jwtTokenEpiryDate = decodedValue.ValidTo;
                DateTime jwtTokenEpiryDateMinusFiveMinutes = jwtTokenEpiryDate.AddMinutes(-5);

                if (jwtTokenEpiryDateMinusFiveMinutes > DateTime.UtcNow)
                {
                    token = Constants.Application.SQLRefDbMIToken;
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
                ConfigReader.Get(Constants.Environment.DatabaseId)
            }));
            Constants.Application.SQLRefDbMIToken = token.Token;

            return Constants.Application.SQLRefDbMIToken;
        }

        public virtual DbSet<PartnerProfile> PartnerProfile { get; set; }
        public virtual DbSet<Product> Product { get; set; }
        public virtual DbSet<MemberAccountReferenceData> MemberAccountReferenceData { get; set; }
        public virtual DbSet<DistanceDetails> Distance { get; set; }
        public virtual DbSet<ListOfValues> ListOfValues { get; set; }
        public virtual DbSet<PartnerBooking> PartnerBooking { get; set; }
        public virtual DbSet<PartnerBookingV2> PartnerBookingV2 { get; set; }
        public virtual DbSet<NonVSRetroCabinClassDetails> NonVSRetroCabinClass { get; set; }
        public virtual DbSet<DistanceAndCountryCodeResponse> DistanceAndCountryCodeResponse { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<PartnerProfile>(entity =>
            {
                entity.HasNoKey();

                entity.Property(e => e.RowId)
                    .HasColumnName("RowID")
                    .HasMaxLength(50);

                entity.Property(e => e.AccountTypeCode).HasMaxLength(30);

                entity.Property(e => e.AutomatedRetroPartner)
                    .HasMaxLength(1)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.CreatedBy).HasMaxLength(50);

                entity.Property(e => e.CreatedOn).HasColumnType("datetime");

                entity.Property(e => e.CurrencyCode).HasMaxLength(20);

                entity.Property(e => e.Description).HasMaxLength(255);

                entity.Property(e => e.LastUpdatedBy).HasMaxLength(50);

                entity.Property(e => e.LastUpdatedOn).HasColumnType("datetime");

                entity.Property(e => e.Name).HasMaxLength(100);

                entity.Property(e => e.PartnerCategory).HasMaxLength(30);

                entity.Property(e => e.PartnerCode).HasMaxLength(50);

                entity.Property(e => e.PartnerTerritoryCode).HasMaxLength(50);

                entity.Property(e => e.PartnerType).HasMaxLength(30);

                entity.Property(e => e.SourceIdentifier).HasMaxLength(50);
            });

            modelBuilder.Entity<ListOfValues>(entity =>
            {
                entity.HasKey(e => e.RowId);                   

                entity.Property(e => e.RowId)
                    .HasColumnName("RowID")
                    .HasMaxLength(50);

                entity.Property(e => e.Active)
                    .IsRequired()
                    .HasMaxLength(1)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.CreatedBy).HasMaxLength(50);

                entity.Property(e => e.CreatedOn).HasColumnType("datetime");

                entity.Property(e => e.Description).HasMaxLength(255);

                entity.Property(e => e.LastUpdatedBy).HasMaxLength(50);

                entity.Property(e => e.LastUpdatedOn).HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Parent).HasMaxLength(50);

                entity.Property(e => e.Product).HasMaxLength(200);

                entity.Property(e => e.SubType).HasMaxLength(200);

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Value)
                    .IsRequired()
                    .HasMaxLength(200);
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasNoKey();

                entity.Property(e => e.RowId)
                    .HasColumnName("RowID")
                    .HasMaxLength(50);

                entity.Property(e => e.ProductCategory).HasMaxLength(30);

                entity.Property(e => e.Description).HasMaxLength(255);

                entity.Property(e => e.ProductName)
                   .IsRequired()
                   .HasMaxLength(100);

                entity.Property(e => e.ProductType).HasMaxLength(30);

                entity.Property(e => e.DivisionCode).HasMaxLength(40);

                entity.Property(e => e.AllowCancellation)
                    .HasMaxLength(1);

                entity.Property(e => e.CreatedBy)
                    .HasMaxLength(100);

                entity.Property(e => e.CreatedOn).HasColumnType("datetime");

                entity.Property(e => e.LastUpdatedBy)
                    .HasMaxLength(100);

                entity.Property(e => e.LastUpdatedOn).HasColumnType("datetime");

                entity.Property(e => e.SourceIdentifier)
                    .HasMaxLength(50);

                entity.Property(e => e.SourceSystem)
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<MemberAccountReferenceData>(entity =>
            {
                entity.HasNoKey();
                entity.Property(e => e.TierPointsForRetainToGold);
                entity.Property(e => e.TierPointsForRetainToSilver);
                entity.Property(e => e.TierPointsForRedToSilver);
                entity.Property(e => e.TierPointsForSilverToGold);
            });

            modelBuilder.Entity<DistanceDetails>(entity =>
            {
                entity.HasNoKey();
                entity.Property(e => e.DistanceInMiles);
            });
            modelBuilder.Entity<PartnerBooking>(entity =>
            {
                entity.HasNoKey();
                entity.Property(e => e.CabinClass);
                entity.Property(e => e.BookClass);
                entity.Property(e => e.OriginCountryCode);
                entity.Property(e => e.DestinationCountryCode);
                entity.Property(e => e.DistanceInMiles);
            });
            modelBuilder.Entity<PartnerBookingV2>(entity =>
            {
                entity.HasNoKey();
                entity.Property(e => e.CabinClass);
                entity.Property(e => e.BookClass);
                entity.Property(e => e.OriginCountryCode);
                entity.Property(e => e.DestinationCountryCode);
                entity.Property(e => e.DistanceInMiles);
                entity.Property(e => e.Type);
                entity.Property(e => e.Value);
                entity.Property(e => e.RetroClaimPeriod);
                entity.Property(e => e.AgreementStartDate);
                entity.Property(e => e.AgreementEndDate);
                entity.Property(e => e.RetroClaimMinimumPeriod);
                entity.Property(e => e.PartnerCode);
                entity.Property(e => e.RedemptionClassIndicator);
                entity.Property(e => e.BookingClass);
                entity.Property(e => e.StartDate);
                entity.Property(e => e.EndDate);
            });
            modelBuilder.Entity<NonVSRetroCabinClassDetails>(entity =>
            {
                entity.HasNoKey();
                entity.Property(e => e.OperatingBookingClass);
                entity.Property(e => e.CabinClass);
            });
            modelBuilder.Entity<DistanceAndCountryCodeResponse>(entity =>
            {
                entity.HasNoKey();
                entity.Property(e => e.DistanceInMiles);
                entity.Property(e => e.OriginCountryCode);
                entity.Property(e => e.DestinationCountryCode);
            });

        }

        public virtual void OnModelCreatingPartial(ModelBuilder modelBuilder) { }
    }
}
