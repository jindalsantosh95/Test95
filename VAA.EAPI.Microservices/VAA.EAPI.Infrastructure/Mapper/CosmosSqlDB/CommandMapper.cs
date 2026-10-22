using VAA.EAPI.Infrastructure.Repository.CosmosSqlDB;
using VAA.EAPI.Infrastructure.Services.CosmosSqlDB;

namespace VAA.EAPI.Infrastructure.Mapper.CosmosSqlDB
{
    public class CommandMapper
    {
        public static CosmosReaderServiceCommand MapCosmosReaderCommand(CosmosReaderRepoCommand cosmosReaderRepoCommand)
        {
            return new CosmosReaderServiceCommand
            {
                CollectionName = cosmosReaderRepoCommand.CollectionName,
                DatabaseEndpoint = cosmosReaderRepoCommand.DatabaseEndpoint,
                DatabaseName = cosmosReaderRepoCommand.DatabaseName,
                QueryParams = cosmosReaderRepoCommand.QueryParams,
                SqlQuery = cosmosReaderRepoCommand.SqlQuery
            };
        }

    }
}
