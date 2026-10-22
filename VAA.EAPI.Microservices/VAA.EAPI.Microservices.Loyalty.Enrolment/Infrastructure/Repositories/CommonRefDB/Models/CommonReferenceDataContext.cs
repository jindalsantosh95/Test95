using Azure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace VAA.EAPI.Microservices.Loyalty.Enrolment.Infrastructure.Repositories.CommonRefDB.Models
{
    public class CommonReferenceDataContext : DbContext
    {
        private readonly ILogger<CommonReferenceDataContext> _logger;
        readonly StringBuilder _logText = new StringBuilder();
        public CommonReferenceDataContext()
        {
        }

        public CommonReferenceDataContext(DbContextOptions<CommonReferenceDataContext> options, ILogger<CommonReferenceDataContext> logger)
            : base(options)
        {
            _logger = logger;
            GetSQLConnection();
        }

        private void GetSQLConnection()
        {
            _logText.AppendLine($"Common Ref DB token Start");
            Microsoft.Data.SqlClient.SqlConnection Conn = (Microsoft.Data.SqlClient.SqlConnection)Database.GetDbConnection();
            string token;
            if (string.IsNullOrEmpty(Constants.Application.CommonSQLRefDBMIToken))
            {
                token = GetCommonRefDBMIToken();
                _logText.AppendLine($"Common Ref DB token not present in Cache || Created using Managed Identity");
            }
            else
            {
                JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
                JwtSecurityToken decodedValue = handler.ReadJwtToken(Constants.Application.CommonSQLRefDBMIToken);
                DateTime jwtTokenEpiryDate = decodedValue.ValidTo;
                DateTime jwtTokenEpiryDateMinusFiveMinutes = jwtTokenEpiryDate.AddMinutes(-5);
                if (jwtTokenEpiryDate != null && jwtTokenEpiryDateMinusFiveMinutes > DateTime.UtcNow)
                {
                    _logText.AppendLine($"Common Ref DB token fetched from cache");
                    token = Constants.Application.CommonSQLRefDBMIToken;
                }
                else
                {
                    token = GetCommonRefDBMIToken();
                    _logText.AppendLine($"Common Ref DB token expired || Created using Managed Identity");
                }
            }

            Conn.AccessToken = token;
            _logText.AppendLine($"Common Ref DB token End");
            _logger.LogInformation(_logText.ToString());
        }

        private string GetCommonRefDBMIToken()
        {
            DefaultAzureCredentialOptions opt = new DefaultAzureCredentialOptions() { ExcludeSharedTokenCacheCredential = true };
            DefaultAzureCredential credential = new DefaultAzureCredential(opt);
            Azure.Core.AccessToken token = credential.GetToken(new Azure.Core.TokenRequestContext(new[] {
                ConfigReader.Get(Constants.Environment.DatabaseID)
            }));
            Constants.Application.CommonSQLRefDBMIToken = token.Token;

            return Constants.Application.CommonSQLRefDBMIToken;
        }

        public virtual DbSet<Campaigns> Campaigns { get; set; }
        public virtual DbSet<Distance> Distance { get; set; }
        public virtual DbSet<ListOfValues> ListOfValues { get; set; }
        public virtual DbSet<MediaCodes> MediaCodes { get; set; }
        public virtual DbSet<PartnerAirport> PartnerAirport { get; set; }
        public virtual DbSet<PartnerProfile> PartnerProfile { get; set; }
        public virtual DbSet<PartnerStatusCodes> PartnerStatusCodes { get; set; }
        public virtual DbSet<PartnerTerms> PartnerTerms { get; set; }
        public virtual DbSet<PaternerProductOffering> PaternerProductOffering { get; set; }
        public virtual DbSet<Product> Product { get; set; }
        public virtual DbSet<ProgramMaster> ProgramMaster { get; set; }
        public virtual DbSet<ProgramPointTypeConfiguration> ProgramPointTypeConfiguration { get; set; }
        public virtual DbSet<ProgramProfile> ProgramProfile { get; set; }
        public virtual DbSet<ProgramSubTypes> ProgramSubTypes { get; set; }
        public virtual DbSet<RequiredListOfValues> RequiredListOfValues { get; set; }
        public virtual DbSet<SiebelListOfValues> SiebelListOfValues { get; set; }
        public virtual DbSet<StatusOfMemberRestrictions> StatusOfMemberRestrictions { get; set; }
        public virtual DbSet<TierMaster> TierMaster { get; set; }
        public virtual DbSet<TitleMaster> TitleMaster { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Campaigns>(entity =>
            {
                entity.HasKey(e => e.RowId)
                    .HasName("PK__Campaign__FFEE745108C246A7");

                entity.Property(e => e.RowId)
                    .HasColumnName("RowID")
                    .HasMaxLength(50);

                entity.Property(e => e.CampaignCode)
                    .IsRequired()
                    .HasMaxLength(30);

                entity.Property(e => e.CampaignEndDate).HasColumnType("datetime");

                entity.Property(e => e.CampaignPurpose).HasMaxLength(100);

                entity.Property(e => e.CampaignStartDate).HasColumnType("datetime");

                entity.Property(e => e.CreatedBy).HasMaxLength(50);

                entity.Property(e => e.CreatedOn).HasColumnType("datetime");

                entity.Property(e => e.DblastUpdatedOn)
                    .HasColumnName("DBLastUpdatedOn")
                    .HasColumnType("datetime");

                entity.Property(e => e.DblastpdatedSrc)
                    .HasColumnName("DBLastpdatedSrc")
                    .HasMaxLength(50);

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

                entity.Property(e => e.StatusCode).HasMaxLength(30);

                entity.Property(e => e.TemplateUrl)
                    .HasColumnName("TemplateURL")
                    .HasMaxLength(2000);

                entity.Property(e => e.WebRegistrationRequired)
                    .IsRequired()
                    .HasMaxLength(1)
                    .IsUnicode(false)
                    .IsFixedLength();
            });

            modelBuilder.Entity<Distance>(entity =>
            {
                entity.HasKey(e => e.RowId)
                    .HasName("PK__Distance__FFEE74517F4202B0");

                entity.Property(e => e.RowId)
                    .HasColumnName("RowID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedBy)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedOn).HasColumnType("datetime");

                entity.Property(e => e.Destination)
                    .IsRequired()
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.DestinationCountryCode)
                    .IsRequired()
                    .HasMaxLength(75)
                    .IsUnicode(false);

                entity.Property(e => e.DestinationCountryName)
                    .HasMaxLength(75)
                    .IsUnicode(false);

                entity.Property(e => e.DistanceInMiles)
                    .IsRequired()
                    .HasMaxLength(22)
                    .IsUnicode(false);

                entity.Property(e => e.DistanceUom)
                    .HasColumnName("DistanceUOM")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.LastUpdatedBy)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.LastUpdatedOn).HasColumnType("datetime");

                entity.Property(e => e.Origin)
                    .IsRequired()
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.OriginCountryCode)
                    .IsRequired()
                    .HasMaxLength(75)
                    .IsUnicode(false);

                entity.Property(e => e.OriginCountryName)
                    .HasMaxLength(75)
                    .IsUnicode(false);

                entity.Property(e => e.SourceIdentifier)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SourceSystem)
                    .HasMaxLength(100)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<ListOfValues>(entity =>
            {
                entity.HasKey(e => e.RowId)
                    .HasName("PK__SiebelLi__FFEE74513B65449B");

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

            modelBuilder.Entity<MediaCodes>(entity =>
            {
                entity.HasKey(e => e.RowId)
                    .HasName("PK__MediaCod__FFEE7451114DBE72");

                entity.Property(e => e.RowId)
                    .HasColumnName("RowID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ActivityStatement)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedBy)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedOn).HasColumnType("datetime");

                entity.Property(e => e.LastUpdatedBy)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.LastUpdatedOn).HasColumnType("datetime");

                entity.Property(e => e.MediaChannelCode)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.MediaCode)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.MediaCodeCategory)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.MediaCodeDescription)
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.Miles).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.SourceIdentifier)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SourceSystem)
                    .HasMaxLength(100)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<PartnerAirport>(entity =>
            {
                entity.HasKey(e => e.RowId)
                    .HasName("PK__PartnerA__FFEE74512B5810A3");

                entity.Property(e => e.RowId)
                    .HasColumnName("RowID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.AirportCode)
                    .IsRequired()
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedOn).HasColumnType("datetime");

                entity.Property(e => e.DblastUpdatedOn)
                    .HasColumnName("DBLastUpdatedOn")
                    .HasColumnType("datetime");

                entity.Property(e => e.DblastUpdatedSrc)
                    .HasColumnName("DBLastUpdatedSrc")
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.LastUpdatedBy)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.LastUpdatedOn).HasColumnType("datetime");

                entity.Property(e => e.PartnerId)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SourceIdentifier)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SourceSystem).HasMaxLength(100);
            });

            modelBuilder.Entity<PartnerProfile>(entity =>
            {
                entity.HasNoKey();

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

                entity.Property(e => e.RowId)
                    .HasColumnName("RowID")
                    .HasMaxLength(50);

                entity.Property(e => e.SourceIdentifier).HasMaxLength(50);
            });

            modelBuilder.Entity<PartnerStatusCodes>(entity =>
            {
                entity.HasKey(e => e.RowId)
                    .HasName("PK__PartnerS__FFEE74513ECF6F2B");

                entity.Property(e => e.RowId)
                    .HasColumnName("RowID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedBy)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedOn).HasColumnType("datetime");

                entity.Property(e => e.Description)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.LastUpdatedBy)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.LastUpdatedOn).HasColumnType("datetime");

                entity.Property(e => e.PartnerId)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SourceIdentifier)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.StatusCode)
                    .HasMaxLength(30)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<PartnerTerms>(entity =>
            {
                entity.HasKey(e => e.RowId)
                    .HasName("PK__PartnerT__FFEE74510C8D2074");

                entity.Property(e => e.RowId)
                    .HasColumnName("RowID")
                    .HasMaxLength(50);

                entity.Property(e => e.AgreementEndDate).HasColumnType("datetime");

                entity.Property(e => e.AgreementStartDate).HasColumnType("datetime");

                entity.Property(e => e.CreatedBy).HasMaxLength(50);

                entity.Property(e => e.CreatedOn).HasColumnType("datetime");

                entity.Property(e => e.DblastUpdatedOn)
                    .HasColumnName("DBLastUpdatedOn")
                    .HasColumnType("datetime");

                entity.Property(e => e.DblastUpdatedSrc)
                    .HasColumnName("DBLastUpdatedSrc")
                    .HasMaxLength(50);

                entity.Property(e => e.LastUpdatedBy).HasMaxLength(50);

                entity.Property(e => e.LastUpdatedOn).HasColumnType("datetime");

                entity.Property(e => e.ProgramId)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.RetroClaimMinimumPeriod).HasColumnType("decimal(10, 2)");

                entity.Property(e => e.SkipFlyingClubAccrualValidations).HasMaxLength(50);

                entity.Property(e => e.SourceIdentifier).HasMaxLength(50);
            });

            modelBuilder.Entity<PaternerProductOffering>(entity =>
            {
                entity.HasKey(e => e.RowId)
                    .HasName("PK__Paterner__FFEE74518FBF507D");

                entity.Property(e => e.RowId)
                    .HasColumnName("RowID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedOn).HasColumnType("datetime");

                entity.Property(e => e.Description)
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.EffectiveFrom).HasColumnType("datetime");

                entity.Property(e => e.EffectiveTo).HasColumnType("datetime");

                entity.Property(e => e.LastUpdatedBy)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.LastUpdatedOn).HasColumnType("datetime");

                entity.Property(e => e.OfferTypeCode)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.PricingMethod)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.ProductId)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.ProgramId)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.SourceIdentifier)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SourceSystem)
                    .HasMaxLength(100)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.RowId)
                    .HasName("PK__Product__FFEE7451A2366314");

                entity.Property(e => e.RowId)
                    .HasColumnName("RowID")
                    .HasMaxLength(50);

                entity.Property(e => e.AllowCancellation)
                    .HasMaxLength(1)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.AwardExpiryPeriodDuration).HasColumnType("decimal(10, 2)");

                entity.Property(e => e.AwardExpiryPeriodType).HasMaxLength(30);

                entity.Property(e => e.AwardPeriodTypeCode).HasMaxLength(30);

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(256)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedOn).HasColumnType("datetime");

                entity.Property(e => e.Description).HasMaxLength(255);

                entity.Property(e => e.DivisionCode).HasMaxLength(40);

                entity.Property(e => e.Duration).HasColumnType("decimal(10, 2)");

                entity.Property(e => e.ExpiryBasis).HasMaxLength(30);

                entity.Property(e => e.ExpiryInYears).HasColumnType("decimal(10, 2)");

                entity.Property(e => e.ExpiryLeadTime).HasColumnType("decimal(10, 2)");

                entity.Property(e => e.ExpiryUnitOfMeasure).HasMaxLength(30);

                entity.Property(e => e.LastUpdatedBy)
                    .IsRequired()
                    .HasMaxLength(256)
                    .IsUnicode(false);

                entity.Property(e => e.LastUpdatedOn).HasColumnType("datetime");

                entity.Property(e => e.ProductCategory).HasMaxLength(30);

                entity.Property(e => e.ProductName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.ProductType).HasMaxLength(30);

                entity.Property(e => e.SequenceTypeCode).HasMaxLength(30);

                entity.Property(e => e.SourceIdentifier)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SourceSystem)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<ProgramMaster>(entity =>
            {
                entity.HasKey(e => e.RowId)
                    .HasName("PK_Program_Row_ID");

                entity.Property(e => e.RowId)
                    .HasColumnName("RowID")
                    .HasMaxLength(50);

                entity.Property(e => e.Active)
                    .IsRequired()
                    .HasMaxLength(1)
                    .IsUnicode(false);

                entity.Property(e => e.ApplyType)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(256)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedOn).HasColumnType("datetime");

                entity.Property(e => e.DblastUpdatedOn)
                    .HasColumnName("DBLastUpdatedOn")
                    .HasColumnType("datetime");

                entity.Property(e => e.DblastUpdatedSrc)
                    .HasColumnName("DBLastUpdatedSrc")
                    .HasMaxLength(256)
                    .IsUnicode(false);

                entity.Property(e => e.EndDate).HasColumnType("datetime");

                entity.Property(e => e.LastUpdatedBy)
                    .IsRequired()
                    .HasMaxLength(256)
                    .IsUnicode(false);

                entity.Property(e => e.LastUpdatedOn).HasColumnType("datetime");

                entity.Property(e => e.PartnerFlag)
                    .HasMaxLength(1)
                    .IsUnicode(false);

                entity.Property(e => e.PartnerId)
                    .HasColumnName("PartnerID")
                    .HasMaxLength(15)
                    .IsUnicode(false);

                entity.Property(e => e.PointExpiryLevel)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.PointsToPayFlag)
                    .HasMaxLength(1)
                    .IsUnicode(false);

                entity.Property(e => e.ProgramDescription)
                    .IsRequired()
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.ProgramName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SourceIdentifier)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SourceSystem)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.StartDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<ProgramPointTypeConfiguration>(entity =>
            {
                entity.HasKey(e => e.RowId)
                    .HasName("PK__ProgramP__FFEE7451B8026F96");

                entity.Property(e => e.RowId)
                    .HasColumnName("RowID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Active)
                    .IsRequired()
                    .HasMaxLength(1)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.AttributeType)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.BusinessInternalName)
                    .HasMaxLength(75)
                    .IsUnicode(false);

                entity.Property(e => e.CancelActionCode)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.CostPerPoint)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedOn).HasColumnType("datetime");

                entity.Property(e => e.DataType)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DefaultValue)
                    .HasMaxLength(75)
                    .IsUnicode(false);

                entity.Property(e => e.Description)
                    .HasMaxLength(1000)
                    .IsUnicode(false);

                entity.Property(e => e.DispalyName)
                    .IsRequired()
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.DisplayToUser)
                    .IsRequired()
                    .HasMaxLength(1)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.InternalName)
                    .IsRequired()
                    .HasMaxLength(75)
                    .IsUnicode(false);

                entity.Property(e => e.LastUpdatedBy)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.LastUpdatedOn).HasColumnType("datetime");

                entity.Property(e => e.MethodName)
                    .HasMaxLength(75)
                    .IsUnicode(false);

                entity.Property(e => e.PointTypeId)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ProgramId)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.QualificationPointFlag)
                    .IsRequired()
                    .HasMaxLength(15)
                    .IsUnicode(false);

                entity.Property(e => e.ReadOnlyFlag)
                    .IsRequired()
                    .HasMaxLength(1)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.SourceIdentifier)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SourceSystem)
                    .HasMaxLength(100)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<ProgramProfile>(entity =>
            {
                entity.HasKey(e => e.RowId)
                    .HasName("PK__ProgramP__FFEE74514DA875A4");

                entity.Property(e => e.RowId)
                    .HasColumnName("RowID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Active)
                    .HasMaxLength(1)
                    .IsUnicode(false);

                entity.Property(e => e.ApplyType)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.Brand)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedBy)
                    .HasMaxLength(256)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedOn).HasColumnType("datetime");

                entity.Property(e => e.EndDate).HasColumnType("datetime");

                entity.Property(e => e.LastUpdatedBy)
                    .HasMaxLength(256)
                    .IsUnicode(false);

                entity.Property(e => e.LastUpdatedOn).HasColumnType("datetime");

                entity.Property(e => e.PartnerFlag)
                    .HasMaxLength(1)
                    .IsUnicode(false);

                entity.Property(e => e.PartnerId)
                    .HasColumnName("PartnerID")
                    .HasMaxLength(15)
                    .IsUnicode(false);

                entity.Property(e => e.PointExpiryLevel)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.PointsToPayFlag)
                    .HasMaxLength(1)
                    .IsUnicode(false);

                entity.Property(e => e.ProgramCode).HasMaxLength(100);

                entity.Property(e => e.ProgramDescription)
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.ProgramName)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SourceIdentifier)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SourceSystem)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.StartDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<ProgramSubTypes>(entity =>
            {
                entity.HasKey(e => e.RowId)
                    .HasName("PK__ProgramS__FFEE74516044DAF5");

                entity.Property(e => e.RowId)
                    .HasColumnName("RowID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedBy)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedOn).HasColumnType("datetime");

                entity.Property(e => e.DefaultFlag)
                    .IsRequired()
                    .HasMaxLength(1)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Description)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.LastUpdatedBy)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.LastUpdatedOn).HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ProgramId)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<RequiredListOfValues>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("RequiredListOfValues", "STG");

                entity.Property(e => e.Type).HasMaxLength(255);
            });

            modelBuilder.Entity<SiebelListOfValues>(entity =>
            {
                entity.HasKey(e => e.RowId)
                    .HasName("PK__SiebelLi__FFEE74515A016ED0");

                entity.ToTable("SiebelListOfValues", "STG");

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

            modelBuilder.Entity<StatusOfMemberRestrictions>(entity =>
            {
                entity.HasKey(e => e.RowId)
                    .HasName("PK__StatusOf__FFEE74510DCDF9E4");

                entity.Property(e => e.RowId)
                    .HasColumnName("RowID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedOn).HasColumnType("datetime");

                entity.Property(e => e.LastUpdatedBy)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.LastUpdatedOn).HasColumnType("datetime");

                entity.Property(e => e.ProgramId)
                    .IsRequired()
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.RestrictCredits)
                    .IsRequired()
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.RestrictDebits)
                    .IsRequired()
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.SourceIdentifier)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SourceSystem)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.StatusCode)
                    .IsRequired()
                    .HasMaxLength(30)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<TierMaster>(entity =>
            {
                entity.HasKey(e => e.RowId)
                    .HasName("PK__TierMast__FFEE7451E09E457F");

                entity.Property(e => e.RowId)
                    .HasColumnName("RowID")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(256)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedOn).HasColumnType("datetime");

                entity.Property(e => e.EffectiveFrom).HasColumnType("datetime");

                entity.Property(e => e.EffectiveTo)
                    .HasMaxLength(256)
                    .IsUnicode(false);

                entity.Property(e => e.ExpirationSnapTo)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.ExpireExtensionDays).HasColumnType("decimal(10, 2)");

                entity.Property(e => e.LastUpdatedBy)
                    .IsRequired()
                    .HasMaxLength(256)
                    .IsUnicode(false);

                entity.Property(e => e.LastUpdatedOn).HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.NonQualifyingPointsExpirationLength).HasColumnType("decimal(10, 2)");

                entity.Property(e => e.NonQualifyingPointsExpirationUoM)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.QualifyingPeriodLength).HasColumnType("decimal(10, 2)");

                entity.Property(e => e.QualifyingPeriodUoM)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.QualifyingPointsExpirationLength)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.QualifyingPointsExpirationUoM).HasColumnType("decimal(10, 2)");

                entity.Property(e => e.RequalifyingPeriodLength).HasColumnType("decimal(10, 2)");

                entity.Property(e => e.RequalifyingPeriodUoM)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.Sequence).HasColumnType("decimal(10, 2)");

                entity.Property(e => e.SourceIdentifier)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SourceSystem)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.StatementFrequency)
                    .HasMaxLength(30)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<TitleMaster>(entity =>
            {
                entity.HasKey(e => e.RowId)
                    .HasName("PK__TitleMas__FFEE74517312543B");

                entity.Property(e => e.RowId)
                    .HasColumnName("RowID")
                    .HasMaxLength(50);

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(e => e.CreatedOn).HasColumnType("datetime");

                entity.Property(e => e.EffectiveFrom).HasColumnType("datetime");

                entity.Property(e => e.EffectiveTo).HasColumnType("datetime");

                entity.Property(e => e.EnvelopeSalutationTemplate)
                    .IsRequired()
                    .HasMaxLength(250);

                entity.Property(e => e.FormalEnvelopeSalutationTemplate)
                    .IsRequired()
                    .HasMaxLength(250);

                entity.Property(e => e.Gender)
                    .HasMaxLength(1)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.InformalSalutationTemplate).HasMaxLength(250);

                entity.Property(e => e.LastUpdatedBy)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(e => e.LastUpdatedOn).HasColumnType("datetime");

                entity.Property(e => e.Priority).HasColumnType("decimal(10, 2)");

                entity.Property(e => e.SourceIdentifier).HasMaxLength(50);

                entity.Property(e => e.SourceSystem).HasMaxLength(50);

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(30);

                entity.Property(e => e.VisibleToSuperUser)
                    .HasMaxLength(1)
                    .IsUnicode(false)
                    .IsFixedLength();
            });

            OnModelCreatingPartial(modelBuilder);
        }

        public virtual void OnModelCreatingPartial(ModelBuilder modelBuilder) { }
    }
}
