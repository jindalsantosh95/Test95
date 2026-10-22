using Microsoft.EntityFrameworkCore;

namespace VAA.EAPI.Microservices.Retail.Ndc.Validation.Infrastructure.Models;

public partial class AgencyDataContext : DbContext
{
    public AgencyDataContext()
    {
    }

    public AgencyDataContext(DbContextOptions<AgencyDataContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AgencyData> VwIdxKeyValidations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("Latin1_General_CI_AS");

        modelBuilder.Entity<AgencyData>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vwIDX_KEY_VALIDATION");

            entity.Property(e => e.AccountCode).HasMaxLength(50);
            entity.Property(e => e.AgencyId).HasMaxLength(15);
            entity.Property(e => e.AgencyIdType).HasMaxLength(20);
            entity.Property(e => e.AirlineTicketingAuthorityInd).HasColumnName("airlineTicketingAuthorityInd");
            entity.Property(e => e.BspActiveStatusInd).HasColumnName("bspActiveStatusInd");
            entity.Property(e => e.BusinessType).HasMaxLength(50);
            entity.Property(e => e.CardFopAllowedInd).HasColumnName("cardFopAllowedInd");
            entity.Property(e => e.CashFopAllowedInd).HasColumnName("cashFopAllowedInd");
            entity.Property(e => e.CompanyName).HasMaxLength(150);
            entity.Property(e => e.CorporateAccountCode).HasMaxLength(50);
            entity.Property(e => e.CorporateId).HasMaxLength(20);
            entity.Property(e => e.CountryCode).HasMaxLength(2);
            entity.Property(e => e.EasyPayFopAllowedInd).HasColumnName("easyPayFopAllowedInd");
            entity.Property(e => e.Environment)
                .HasMaxLength(50)
                .HasColumnName("environment");
            entity.Property(e => e.ExpiryDateKey)
                .HasColumnType("datetime")
                .HasColumnName("expiryDateKey");
            entity.Property(e => e.ExpiryDateKey2)
                .HasColumnType("datetime")
                .HasColumnName("expiryDateKey2");
            entity.Property(e => e.KeyId).HasColumnName("keyId");
            entity.Property(e => e.KeyStatus)
                .HasMaxLength(20)
                .HasColumnName("keyStatus");
            entity.Property(e => e.KeyType)
                .HasMaxLength(50)
                .HasColumnName("keyType");
            entity.Property(e => e.LastUpdate).HasColumnType("datetime");
            entity.Property(e => e.LastUpdateIATA).HasColumnType("datetime");
            entity.Property(e => e.ParentId).HasColumnName("parentId");
            entity.Property(e => e.PciDssComplianceInd).HasColumnName("pciDssComplianceInd");
            entity.Property(e => e.RequestAgencyId)
                .HasMaxLength(15)
                .HasColumnName("requestAgencyId");
            entity.Property(e => e.RequestAgencyIdType)
                .HasMaxLength(20)
                .HasColumnName("requestAgencyIdType");
            entity.Property(e => e.RequestOrgName)
                .HasMaxLength(150)
                .HasColumnName("requestOrgName");
            entity.Property(e => e.RequestOrgStatus)
                .HasMaxLength(15)
                .HasColumnName("requestOrgStatus");
            entity.Property(e => e.RowId).HasColumnName("rowId");
            entity.Property(e => e.Status).HasMaxLength(15);
            entity.Property(e => e.SubscriberKey)
                .HasMaxLength(150)
                .HasColumnName("subscriberKey");
            entity.Property(e => e.SubscriberKey2)
                .HasMaxLength(150)
                .HasColumnName("subscriberKey2");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
