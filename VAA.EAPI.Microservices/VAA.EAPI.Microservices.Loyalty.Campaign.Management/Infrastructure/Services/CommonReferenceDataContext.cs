#nullable disable

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Text;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Helper;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Models;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Services
{
    public class CommonReferenceDataContext : DbContext
    {
        private readonly ILogger<CommonReferenceDataContext> _logger;
        readonly StringBuilder _logText = new StringBuilder();

        public CommonReferenceDataContext(DbContextOptions<CommonReferenceDataContext> options, ILogger<CommonReferenceDataContext> logger)
            : base(options)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            SqlConnection Conn = (SqlConnection)Database.GetDbConnection();
            ConnectionHelper.GetSQLConnection(Conn);
        }

        public CommonReferenceDataContext()
        {
            //added for mocking 
        }

        public virtual DbSet<Models.Campaign> Campaigns { get; set; }

        public virtual DbSet<CampaignsPromotionsMapping> CampaignsPromotionsMapping { get; set; }

        public virtual DbSet<ListOfValues> ListOfValues { get; set; }

        public virtual DbSet<Models.Promotion> Promotions { get; set; }

        public virtual DbSet<Models.PromotionsDownload> PromotionsDownload { get; set; }
        public virtual DbSet<Models.PromotionCampaign> PromotionCampaigns { get; set; }

        public virtual DbSet<CampaignSearchData> CampaignSearchData { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CampaignsPromotionsMapping>()
                .HasNoKey();
            modelBuilder.Entity<Promotion>()
                .HasNoKey();
            modelBuilder.Entity<PromotionCampaign>()
                .HasNoKey();
            modelBuilder.Entity<PromotionsDownload>()
                .HasNoKey();
            modelBuilder.Entity<ListOfValues>().HasNoKey();

            modelBuilder.Entity<CampaignSearchData>().HasNoKey();

            modelBuilder.Entity<Models.Campaign>(entity =>
            {
                entity.HasKey(e => e.RowId)
                    .HasName("PK__Campaign__FFEE745108C246A7");

                entity.Property(e => e.RowId)
                    .HasColumnName("RowID")
                    .HasMaxLength(50);

                entity.Property(e => e.CampaignCode)
                    .IsRequired()
                    .HasMaxLength(30);

                entity.Property(e => e.EndDate).HasColumnType("datetime");

                entity.Property(e => e.CampaignPurpose).HasMaxLength(100);

                entity.Property(e => e.StartDate).HasColumnType("datetime");

                entity.Property(e => e.CreatedBy).HasMaxLength(50);

                entity.Property(e => e.CreatedOn).HasColumnType("datetime");

                entity.Property(e => e.Description).HasMaxLength(255);

                entity.Property(e => e.LastUpdatedBy).HasMaxLength(50);

                entity.Property(e => e.LastUpdatedOn).HasColumnType("datetime");

                entity.Property(e => e.Name).HasMaxLength(100);

                entity.Property(e => e.Purpose).HasMaxLength(2000);

                entity.Property(e => e.RegistrationRequired)
                    .IsRequired()
                    .HasMaxLength(1)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.SaveEmailsSent)
                    .IsRequired()
                    .HasMaxLength(1)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.SourceIdentifier).HasMaxLength(50);

                entity.Property(e => e.SourceSystem).HasMaxLength(100);

                entity.Property(e => e.Status).HasMaxLength(30);

                entity.Property(e => e.WebRegistrationRequired)
                    .IsRequired()
                    .HasMaxLength(1)
                    .IsUnicode(false)
                    .IsFixedLength();
            });
            OnModelCreatingPartial(modelBuilder);
        }

        public virtual void OnModelCreatingPartial(ModelBuilder modelBuilder) { }
    }
}
