namespace VAA.EAPI.Microservices.Insights.Infrastructure.Azure.Models
{
    public class AzureResourceGroup
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Location { get; set; }
        public AzureTags Tags { get; set; }
    }
}
