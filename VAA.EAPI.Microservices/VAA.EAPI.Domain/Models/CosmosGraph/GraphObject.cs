namespace VAA.EAPI.Domain.Models.CosmosGraph
{
    public abstract class GraphObject
    {
        public string Id { get; set; }
        public string Label { get; set; }
        public string PartitionKey { get; set; }
        public string Type { get; set; }
        public abstract PropertyValue GetProperties();
        public string DocumentType { get; set; }
        public string DocumentVersion { get; set; }
    }
}
