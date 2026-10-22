
namespace VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2
{
    public class CosmosRepoBase
    {
        public string DatabaseName { get; set; }
        public string CollectionName { get; set; }
        public string DatabaseEndpoint { get; set; }
    }
}
