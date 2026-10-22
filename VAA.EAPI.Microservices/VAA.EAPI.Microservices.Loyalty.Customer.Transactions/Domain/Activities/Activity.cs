using System;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Base;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Activities
{
    public class Activity : CustomerTransaction
    {
        public string SchemaVersion { get; set; }
        public string SourceIdentifier { get; set; }
        public string SourceSystem { get; set; }
        public DateTime? DueDate { get; set; }
        public string Brand { get; set; }
        public string DocType { get; set; }
        public string Comments { get; set; }
        public string Status { get; set; }
        public string MeetingLocation { get; set; }
        public string Description { get; set; }
        public string ActivityId { get; set; }
        public string ActivityType { get; set; }
        public string TemplateName { get; set; }


        public static explicit operator Activity(ActivityDTO source) => new()
        {
            Id = source.Id,
            CustomerId = source.CustomerId,
            CreatedOn = DateTime.Parse(source.CreatedOn),
            CreatedBy = source.CreatedBy,
            LastUpdatedOn = DateTime.Parse(source.LastUpdatedOn),
            LastUpdatedBy = source.LastUpdatedBy,
            SourceIdentifier = source.SourceIdentifier,
            SourceSystem = source.SourceSystem,
            DueDate = !string.IsNullOrEmpty(source.DueDate) ? DateTime.Parse(source.DueDate) : null,
            Comments = source.Comments,
            Status = source.Status,
            MeetingLocation = source.MeetingLocation,
            Description = source.Description,
            ActivityId = source.ActivityId,
            ActivityType = source.ActivityType,
            TemplateName = source.TemplateName
        };
    }
}
