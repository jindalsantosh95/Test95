using Azure;
using Azure.Data.Tables;

namespace VAA.EAPI.Automation.Core.Helpers.StorageAccount
{
    public sealed class NdcPnrTableEntity : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
        public string Environment { get; set; }
        public string BookingType { get; set; }
        public string PaymentType { get; set; }
        public string SeatType { get; set; }
        public string CreatedUtc { get; set; }
    }
}
