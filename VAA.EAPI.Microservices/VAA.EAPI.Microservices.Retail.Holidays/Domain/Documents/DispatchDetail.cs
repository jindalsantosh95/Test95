using Newtonsoft.Json.Linq;
using System;

namespace VAA.EAPI.Microservices.Retail.Holidays.Domain.Documents
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

        public static explicit operator DispatchDetail(JToken source)
        {
            if (source == null) return null;

            return new()
            {
                DispatchId = source["id"]?.Value<int>() ?? 0,
                DocumentId = source["docId"]?.Value<int>() ?? 0,
                DispatchQueueId = source["dispatchQueueId"]?.Value<int>() ?? 0,
                ToDispatchDateTime = DateTime.TryParse(source["toDispatchDateTime"]?.Value<string>(), out var toDispatchDateTime) ? toDispatchDateTime : null,
                DispatchedDateTime = DateTime.TryParse(source["dispatchedDateTime"]?.Value<string>(), out var dispatchedDateTime) ? dispatchedDateTime : null,
                SendAutomatically = source["sendAutomatically"]?.Value<bool>() ?? false,
                Posted = source["posted"]?.Value<bool>() ?? false,
                DispatchMethod = source["dispatchMethod"]?.Value<string>(),
                DispatchStatus = source["dispatchStatus"]?.Value<string>(),
                Recipient = source["recipient"]?.Value<string>(),
                TrackingNo = source["trackingNo"]?.Value<string>(),
                Description = source["description"]?.Value<string>(),
                Dispatcher = source["dispatcher"]?.Value<string>(),
                Collate = source["collate"]?.Value<int>() ?? 0
            };
        }
    }
}
