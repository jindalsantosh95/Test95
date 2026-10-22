
namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS.Documents
{
    public class DispatchDetail
    {
        public int DispatchId { get; set; }
        public int DocumentId { get; set; }
        public int DispatchQueueId { get; set; }
        public DateTime? ToDispatchDateTime { get; set; }
        public DateTime? DispatchedDateTime { get; set; }
        public bool SendAutomatically { get; set; }
        public bool Posted { get; set; }
        public string DispatchMethod { get; set; }
        public string DispatchStatus { get; set; }
        public string Recipient { get; set; }
        public string TrackingNo { get; set; }
        public string Description { get; set; }
        public string Dispatcher { get; set; }
        public int Collate { get; set; }
    }
}
