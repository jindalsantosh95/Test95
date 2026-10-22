using Microsoft.EntityFrameworkCore;

namespace VAA.EAPI.Microservices.Retail.Ndc.Validation.Infrastructure.Models;

public partial class AgencyOCNDataContext : DbContext
{
    public AgencyOCNDataContext()
    {
    }

    public AgencyOCNDataContext(DbContextOptions<AgencyOCNDataContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AgencyOCNData> VwIdxOcnEndpoints { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("Latin1_General_CI_AS");

        modelBuilder.Entity<AgencyOCNData>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vwIDX_OCN_ENDPOINTS");

            entity.Property(e => e.AgencyClass)
                .HasMaxLength(70)
                .IsFixedLength()
                .HasColumnName("agencyClass");
            entity.Property(e => e.AgencyId).HasMaxLength(15);
            entity.Property(e => e.AgencyIdType).HasMaxLength(20);
            entity.Property(e => e.AuthType).HasMaxLength(25);
            entity.Property(e => e.BusinessType).HasMaxLength(50);
            entity.Property(e => e.ClientKey).HasMaxLength(250);
            entity.Property(e => e.ClientSecret).HasMaxLength(250);
            entity.Property(e => e.CompanyName).HasMaxLength(150);
            entity.Property(e => e.EndPoint).HasMaxLength(150);
            entity.Property(e => e.Environment).HasMaxLength(50);
            entity.Property(e => e.EnvironmentName).HasMaxLength(50);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.KeyType)
                .HasMaxLength(15)
                .HasColumnName("keyType");
            entity.Property(e => e.ParentId).HasColumnName("parentId");
            entity.Property(e => e.Protocol).HasMaxLength(25);
            entity.Property(e => e.RequestAgencyId)
                .HasMaxLength(15)
                .HasColumnName("requestAgencyId");
            entity.Property(e => e.RequestAgencyIdType)
                .HasMaxLength(20)
                .HasColumnName("requestAgencyIdType");
            entity.Property(e => e.RequestOrgId).HasColumnName("requestOrgId");
            entity.Property(e => e.RequestOrgIdParent).HasColumnName("requestOrgIdParent");
            entity.Property(e => e.RequestOrgName)
                .HasMaxLength(150)
                .HasColumnName("requestOrgName");
            entity.Property(e => e.RequestOrgStatus)
                .HasMaxLength(15)
                .HasColumnName("requestOrgStatus");
            entity.Property(e => e.SoapAction).HasMaxLength(80);
            entity.Property(e => e.Status).HasMaxLength(15);
            entity.Property(e => e.TokenUrl).HasMaxLength(150);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
