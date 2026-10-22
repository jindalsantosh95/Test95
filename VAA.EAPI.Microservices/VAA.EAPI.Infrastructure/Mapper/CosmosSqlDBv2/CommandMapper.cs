using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Infrastructure.Services.CosmosSqlDBv2;

namespace VAA.EAPI.Infrastructure.Mapper.CosmosSqlDBv2
{
    public class CommandMapper
    {
        public static CosmosReaderServiceQuery MapCosmosReaderCommand(CosmosReaderRepoQuery cosmosReaderRepoQuery)
        {
            return new CosmosReaderServiceQuery
            {
                CollectionName = cosmosReaderRepoQuery.CollectionName,
                DatabaseEndpoint = cosmosReaderRepoQuery.DatabaseEndpoint,
                DatabaseName = cosmosReaderRepoQuery.DatabaseName,
                QueryParams = cosmosReaderRepoQuery.QueryParams,
                SqlQuery = cosmosReaderRepoQuery.SqlQuery
            };
        }

        public static CosmosModifyServiceCommand MapCosmosModifyCommand(CosmosModifyRepoCommand cosmosReaderRepoCommand)
        {
            return new CosmosModifyServiceCommand
            {
                CollectionName = cosmosReaderRepoCommand.CollectionName,
                DatabaseEndpoint = cosmosReaderRepoCommand.DatabaseEndpoint,
                DatabaseName = cosmosReaderRepoCommand.DatabaseName,
                Event = cosmosReaderRepoCommand.Event,
            };
        }
    }
}
