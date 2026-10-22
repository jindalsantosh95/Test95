using Microsoft.Azure.Cosmos.Table;


namespace VAA.EAPI.Infrastructure.Entity
{
    public class EventStoreDatabaseEntity : TableEntity
    {
        public string ID { get; set; }
        public string SpecVersion { get; set; }
        public string Type { get; set; }
        public string Source { get; set; }
        public string Time { get; set; }
        public string Subject { get; set; }
        public string DataSchema { get; set; }
        public string PayLoad { get; set; }

    }
}
