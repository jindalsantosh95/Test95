using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.BirthdayBonus;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.BirthdayEmail;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.PromotionEnrolledMembers;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.WingsGoldMemberAssessment;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Helper;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services
{
    public class ReportingDataContext : DbContext
    {
        private readonly ILogger<ReportingDataContext> _logger;

        public ReportingDataContext(DbContextOptions<ReportingDataContext> options, ILogger<ReportingDataContext> logger)
            : base(options)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            SqlConnection Conn = (SqlConnection)Database.GetDbConnection();
            ConnectionHelper.GetSQLConnection(Conn);
        }

        public ReportingDataContext()
        {

        }

        public virtual DbSet<MemberContactData> MemberEmailDetails { get; set; }

        public virtual DbSet<BirthdayBonusMemberDetails> BirthdayBonusMemberDetails { get; set; }
        public virtual DbSet<GoldMemberDetails> GoldMemberDetails { get; set; }
        public virtual DbSet<PromotionEnrolledMembers> EnrolledMemberDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MemberContactData>(entity =>
            {
                entity.HasNoKey();
                entity.Property(e => e.FirstName);
                entity.Property(e => e.LastName);
                entity.Property(e => e.MemberNumber);
                entity.Property(e => e.Email);
            });

            modelBuilder.Entity<BirthdayBonusMemberDetails>(entity =>
            {
                entity.HasNoKey();
                entity.Property(e => e.CustomerId);
                entity.Property(e => e.MemberNumber);
            });
            modelBuilder.Entity<GoldMemberDetails>(entity =>
            {
                entity.HasNoKey();
                entity.Property(e => e.CustomerId);
                entity.Property(e => e.LoyaltyMemberNumber);
            });
            modelBuilder.Entity<PromotionEnrolledMembers>(entity =>
            {
                entity.HasNoKey();
                entity.Property(e => e.MemberNumber);
                entity.Property(e => e.Name);
            });
        }
    }
}
