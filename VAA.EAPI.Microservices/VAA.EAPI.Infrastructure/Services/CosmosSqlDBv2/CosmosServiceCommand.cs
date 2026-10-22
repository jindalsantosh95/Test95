namespace VAA.EAPI.Infrastructure.Services.CosmosSqlDBv2
{
    public class CosmosServiceBase
    {
        public string DatabaseName { get; set; }
        public string CollectionName { get; set; }
        public string DatabaseEndpoint { get; set; }
    }
}
