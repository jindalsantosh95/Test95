#nullable disable

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.MemberAccountDetails;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.AutoAccrual;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Helper;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Models;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services
{
    public class CommonReferenceDataContext : DbContext
    {
        private readonly ILogger<CommonReferenceDataContext> _logger;

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
        public virtual DbSet<MemberAccountReferenceData> MemberAccountReferenceData { get; set; }

        public virtual DbSet<PartnerProfile> PartnerProfile { get; set; }

        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<ValidationResult> AutoAccrualValidationResults { get; set; }
        public virtual DbSet<AutoAccrualPartnerTerms> AutoAccrualPartnerTerms { get; set; }

        public virtual DbSet<Distance> Distance { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MemberAccountReferenceData>(entity =>
            {
                entity.HasNoKey();
                entity.Property(e => e.TierPointsForRetainToGold);
                entity.Property(e => e.TierPointsForRetainToSilver);
                entity.Property(e => e.TierPointsForRedToSilver);
                entity.Property(e => e.TierPointsForSilverToGold);
            });

            modelBuilder.Entity<PartnerProfile>().HasNoKey();
            modelBuilder.Entity<Product>().HasNoKey();
            modelBuilder.Entity<ValidationResult>().HasNoKey();
            modelBuilder.Entity<AutoAccrualPartnerTerms>().HasNoKey();
            modelBuilder.Entity<Distance>().HasNoKey();
        }
    }
}
