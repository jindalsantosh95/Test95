using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Constants;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Activities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities
{
    public class ActivityDTO : CosmosDocument
    {
        public string SchemaVersion { get; set; }
        public string Brand { get; set; }
        public string DocType { get; set; }
        public string CustomerId { get; set; }
        public string CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public string LastUpdatedOn { get; set; }
        public string LastUpdatedBy { get; set; }
        public string SourceIdentifier { get; set; }
        public string SourceSystem { get; set; }
        public string DueDate { get; set; }
        public string Comments { get; set; }
        public string Status { get; set; }
        public string MeetingLocation { get; set; }
        public string Description { get; set; }
        public string ActivityId { get; set; }
        public string ActivityType { get; set; }
        public string TemplateName { get; set; }


        public static explicit operator ActivityDTO(Activity activity)
        {
            return new()
            {
                SchemaVersion = Constants.Application.ActivityDocSchemaVersion,
                Brand = Constants.Application.ActivityMemberBrand,
                DocType = ConfigReader.Get(EnvironmentSettings.ActivityDocType),

                Id = activity.Id,
                CreatedBy = activity.CreatedBy,
                CreatedOn = activity.CreatedOn.ToUniversalTime().ToString(Constants.Application.ISODateTimeFormat),
                LastUpdatedBy = activity.LastUpdatedBy,
                LastUpdatedOn = activity.LastUpdatedOn.ToUniversalTime().ToString(Constants.Application.ISODateTimeFormat),
                CustomerId = activity.CustomerId,
                SourceIdentifier = activity.SourceIdentifier ?? string.Empty,
                SourceSystem = activity.SourceSystem ?? string.Empty,
                DueDate = activity.DueDate?.ToUniversalTime().ToString(Constants.Application.ISODateTimeFormat),
                Comments = activity.Comments ?? null,
                Status = activity.Status ?? string.Empty,
                MeetingLocation = activity.MeetingLocation ?? string.Empty,
                Description = activity.Description ?? string.Empty,
                ActivityId = activity.ActivityId ?? string.Empty,
                ActivityType = activity.ActivityType ?? string.Empty,
                TemplateName = activity.TemplateName ?? string.Empty
            };
        }
    }
}
