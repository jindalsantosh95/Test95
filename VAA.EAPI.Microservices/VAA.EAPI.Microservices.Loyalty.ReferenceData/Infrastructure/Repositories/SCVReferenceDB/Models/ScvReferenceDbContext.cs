using Azure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.IdentityModel.Tokens.Jwt;
using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Microservices.Loyalty.ReferenceData.Infrastructure.Repositories.SCVReferenceDB.Models
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
        public virtual DbSet<TitleMaster> TitleMaster { get; set; }

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

                entity.Property(e => e.CategoryCode)
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<TitleMaster>(entity =>
            {
                entity.HasNoKey();
                entity.ToTable("TitleMaster");

                entity.Property(e => e.EnvelopeSalutationTemplate).HasMaxLength(250);
                entity.Property(e => e.FormalEnvelopeSalutationTemplate).HasMaxLength(250);
                entity.Property(e => e.Gender)
                    .HasMaxLength(1)
                    .IsUnicode(false)
                    .IsFixedLength();
                entity.Property(e => e.InformalSalutationTemplate).HasMaxLength(250);
                entity.Property(e => e.Priority).HasColumnType("decimal(10, 2)");
                entity.Property(e => e.Title).HasMaxLength(30);
            });
        }

        public virtual void OnModelCreatingPartial(ModelBuilder modelBuilder) { }
    }
}
