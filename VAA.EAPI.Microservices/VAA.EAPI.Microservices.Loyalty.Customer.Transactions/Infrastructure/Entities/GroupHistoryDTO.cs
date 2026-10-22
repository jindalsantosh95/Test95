using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Group;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities
{
    public class GroupHistoryDTO
    {
        public string HistoryId { get; set; }
        public string CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public string LastUpdatedOn { get; set; }
        public string LastUpdatedBy { get; set; }
        public string StartDate { get; set; }
        public string ExitDate { get; set; }
        public string SourceIdentifier { get; set; }
        public string SourceSystem { get; set; }
        public bool HoHManaged { get; set; }
        public string CustomerId { get; set; }

        public GroupHistoryDTO()
        {

        }

        public GroupHistoryDTO(GroupHistory history)
        {
            HistoryId = history.HistoryId ?? string.Empty;
            CreatedOn = history.CreatedOn?.ToString(Constants.Application.ISODateTimeFormat);
            CreatedBy = history.CreatedBy ?? string.Empty;
            LastUpdatedOn = history.LastUpdatedOn?.ToString(Constants.Application.ISODateTimeFormat);
            LastUpdatedBy = history.LastUpdatedBy ?? string.Empty;
            StartDate = history.StartDate?.ToString(Constants.Application.ISODateTimeFormat);
            ExitDate = history.ExitDate != null ? history.ExitDate?.ToString(Constants.Application.ISODateTimeFormat) : string.Empty;
            SourceIdentifier = history.SourceIdentifier ?? string.Empty;
            SourceSystem = history.SourceSystem ?? string.Empty;
            HoHManaged =history.HoHManaged;
            CustomerId = history.CustomerId ?? string.Empty;
        }
    }
}
