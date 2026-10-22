using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.CampaignEnrolmentDoc;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities
{
    public class CampaignEnrolmentDTO : CosmosDocument
    {
        public string SchemaVersion { get; set; }
        public string Brand { get; set; }
        public string DocType { get; set; }
        public string CustomerId { get; set; }
        public string SourceIdentifier { get; set; }
        public string SourceSystem { get; set; }
        public string CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public string LastUpdatedOn { get; set; }
        public string LastUpdatedBy { get; set; }
        public string MemberNumber { get; set; }
        public string Eligible { get; set; }
        public string OptInDate { get; set; }
        public string EnrollmentExpiryDate { get; set; }
        public string EnrollmentId { get; set; }
        public string PromotionId { get; set; }

        public CampaignEnrolmentDTO()
        {
            // Required for serialisation
        }


        public CampaignEnrolmentDTO(CampaignEnrolment tmDoc)
        {
            SchemaVersion = Constants.Application.TargetedMembershipDocSchemaVersion;
            Brand = Constants.Application.TargetedMembershipBrand;
            DocType = ConfigReader.Get(Constants.EnvironmentSettings.CampaignEnrolmentDocType);

            Id = tmDoc.Id ?? string.Empty;
            EnrollmentId = tmDoc.Id ?? string.Empty;
            PartitionKey = tmDoc.CustomerId ?? string.Empty;
            CustomerId = tmDoc.CustomerId ?? string.Empty;
            CreatedOn = tmDoc.CreatedOn.ToUniversalTime().ToString(Constants.Application.ISODateTimeFormat) ?? string.Empty;
            CreatedBy = tmDoc.CreatedBy ?? string.Empty;
            LastUpdatedOn = tmDoc.LastUpdatedOn.ToUniversalTime().ToString(Constants.Application.ISODateTimeFormat) ?? string.Empty;
            LastUpdatedBy = tmDoc.CreatedBy ?? string.Empty;

            SourceIdentifier = string.Empty;
            SourceSystem = string.Empty;

            MemberNumber = tmDoc.MemberNumber ?? string.Empty;
            OptInDate = tmDoc.CreatedOn.ToUniversalTime().ToString(Constants.Application.ISODateTimeFormat);
            EnrollmentExpiryDate = tmDoc.EnrollmentExpiryDate?.ToUniversalTime().Date.ToString(Constants.Application.ISODateTimeFormat);
            Eligible = tmDoc.Eligible ?? string.Empty;
            PromotionId = tmDoc.PromotionId ?? string.Empty;

        }

        public CampaignEnrolment ToCampaignEnrolment()
        {
            return new CampaignEnrolment(this);
        }
    }
}
