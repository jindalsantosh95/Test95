using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Constants;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.TargetedMembership;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities
{
    public class TargetedMembershipDTO : CosmosDocument
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
        public string AssignmentDate { get; set; }
        public string Comments { get; set; }
        public string CompletedDate { get; set; }
        public string ContactedDate { get; set; }
        public string ProfilingCampaignSourceId { get; set; }
        public string StatusCode { get; set; }
        public string TargetedMembershipId { get; set; }

        public TargetedMembershipDTO()
        {
            // Required for serialisation
        }


        public TargetedMembershipDTO(TargetedMembership tmDoc)
        {
            SchemaVersion = Constants.Application.TargetedMembershipDocSchemaVersion;
            Brand = Constants.Application.TargetedMembershipBrand;
            DocType = ConfigReader.Get(EnvironmentSettings.TargetedMembershipDocType);

            Id = tmDoc.Id;
            PartitionKey = tmDoc.CustomerId;
            CustomerId = tmDoc.CustomerId;
            CreatedBy = tmDoc.CreatedBy;
            CreatedOn = tmDoc.CreatedOn.ToUniversalTime().ToString(Constants.Application.ISODateTimeFormat);
            LastUpdatedOn = tmDoc.LastUpdatedOn.ToUniversalTime().ToString(Constants.Application.ISODateTimeFormat);

            SourceIdentifier = string.Empty;
            SourceSystem = string.Empty;

            AssignmentDate = tmDoc.AssignmentDate?.ToUniversalTime().Date.ToString(Constants.Application.ISODateTimeFormat);
            Comments = tmDoc.Comments;
            CompletedDate = tmDoc.CompletedDate?.ToUniversalTime().Date.ToString(Constants.Application.ISODateTimeFormat) ?? string.Empty;
            ContactedDate = tmDoc.ContactedDate?.ToUniversalTime().Date.ToString(Constants.Application.ISODateTimeFormat) ?? string.Empty;
            StatusCode = tmDoc.StatusCode;
            TargetedMembershipId = tmDoc.TargetedMembershipId;

        }

        public TargetedMembership ToTargetedMembership()
        {
            return new TargetedMembership(this);
        }
    }
}

