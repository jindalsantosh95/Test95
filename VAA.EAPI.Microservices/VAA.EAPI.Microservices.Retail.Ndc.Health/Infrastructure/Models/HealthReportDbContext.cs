using Microsoft.EntityFrameworkCore;
using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Microservices.Retail.Ndc.Health.Infrastructure.Models;

public partial class HealthReportDbContext : DbContext
{
    public HealthReportDbContext()
    {
    }

    public HealthReportDbContext(DbContextOptions<HealthReportDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<HealthCheckAirShoppingView> VHealthcheckairshopping { get; set; }

    public virtual DbSet<HealthCheckOfferPriceView> VHealthcheckofferprice { get; set; }

    public virtual DbSet<HealthCheckOrderCreateView> VHealthcheckordercreate { get; set; }

    public virtual DbSet<HealthCheckSeatAvailabilityView> VHealthcheckseatavailability { get; set; }

    public virtual DbSet<HealthCheckOrderChangeView> VHealthcheckorderchange { get; set; }

    public virtual DbSet<HealthCheckOrderHistoryView> VHealthcheckorderhistory { get; set; }

    public virtual DbSet<HealthCheckOrderQuoteView> VHealthcheckorderquote { get; set; }

    public virtual DbSet<HealthCheckOrderReshopView> VHealthcheckorderreshop { get; set; }

    public virtual DbSet<HealthCheckOrderListView> VHealthcheckorderlist{ get; set; }

    public virtual DbSet<HealthCheckOrderRetrieveView> VHealthcheckorderretrieve { get; set; }

    public virtual DbSet<HealthCheckOCNView> VHealthcheckocn{ get; set; }

    public virtual DbSet<HealthCheckServiceListView> VHealthcheckservicelist { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)

        => optionsBuilder.UseSqlServer(ConfigReader.Get(Constants.Environment.HealthReportSynapseDbConnString));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("Latin1_General_100_BIN2_UTF8");

        modelBuilder.Entity<HealthCheckAirShoppingView>(entity =>
        {
            entity.ToView("V_HEALTHCHECKAIRSHOPPING");
        });

        modelBuilder.Entity<HealthCheckOfferPriceView>(entity =>
        {
            entity.ToView("V_HEALTHCHECKOFFERPRICE");
        });

        modelBuilder.Entity<HealthCheckOrderCreateView>(entity =>
        {
            entity.ToView("V_HEALTHCHECKORDERCREATE");
        });

        modelBuilder.Entity<HealthCheckSeatAvailabilityView>(entity =>
        {
            entity.ToView("V_HEALTHCHECKSEATAVAILABILITY");
        });

        modelBuilder.Entity<HealthCheckOrderChangeView>(entity =>
        {
            entity.ToView("V_HEALTHCHECKORDERCHANGE");
        });

        modelBuilder.Entity<HealthCheckOrderHistoryView>(entity =>
        {
            entity.ToView("V_HEALTHCHECKORDERHISTORY");
        });

        modelBuilder.Entity<HealthCheckOrderQuoteView>(entity =>
        {
            entity.ToView("V_HEALTHCHECKORDERQUOTE");
        });

        modelBuilder.Entity<HealthCheckOrderReshopView>(entity =>
        {
            entity.ToView("V_HEALTHCHECKORDERRESHOP");
        });

        modelBuilder.Entity<HealthCheckOrderListView>(entity =>
        {
            entity.ToView("[V_HEALTHCHECKORDERLIST]");
        });

        modelBuilder.Entity<HealthCheckOrderRetrieveView>(entity =>
        {
            entity.ToView("[V_HEALTHCHECKORDERRETRIEVE]");
        });

        modelBuilder.Entity<HealthCheckOCNView>(entity =>
        {
            entity.ToView("[V_HEALTHCHECKOCN]");
        });
        modelBuilder.Entity<HealthCheckServiceListView>(entity =>
        {
            entity.ToView("[V_HEALTHCHECKSERVICELIST]");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
