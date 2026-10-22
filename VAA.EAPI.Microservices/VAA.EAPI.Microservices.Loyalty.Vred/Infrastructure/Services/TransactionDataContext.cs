using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using VAA.EAPI.Microservices.Loyalty.Vred.Entities.VredTransactions;
using VAA.EAPI.Microservices.Loyalty.Vred.Helper;


namespace VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Services
{
    public class TransactionDataContext : DbContext
    {        

        public TransactionDataContext(DbContextOptions<TransactionDataContext> options)
            : base(options)
        {            

            SqlConnection Conn = (SqlConnection)Database.GetDbConnection();

            ConnectionHelper.GetSQLConnection(Conn);
        }

        public TransactionDataContext()
        {
            //added for mocking 
        }

        public virtual DbSet<AuditTransactionInfo> MemberAccountReferenceData { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AuditTransactionInfo>(entity =>
            {
                entity.HasNoKey();
                entity.Property(e => e.MembershipNumber);
                entity.Property(e => e.TransactionID);
                entity.Property(e => e.TransactionType);
                entity.Property(e => e.ActivityDate);
                entity.Property(e => e.Status);
                entity.Property(e => e.TransactionProcessedDate);
                entity.Property(e => e.TransactionSource);
                entity.Property(e => e.ServiceRequest);
                entity.Property(e => e.SiebelRequest);
                entity.Property(e => e.SiebelResponse);               
                entity.Property(e => e.Created);
                entity.Property(e => e.CreatedBy);
                entity.Property(e => e.CreatedByEmailId);
                entity.Property(e => e.CreatedByUserName);
            });
        }
    }
}
