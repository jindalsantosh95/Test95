using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Helper;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Models;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Services
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

        public virtual DbSet<AssociatedMemberData> AssociatedMemberData { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AssociatedMemberData>(entity =>
            {
                entity.HasNoKey();
                entity.Property(e => e.MemberNumber);
            });
        }

    }
}
