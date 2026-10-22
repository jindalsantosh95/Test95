using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Infrastructure.Services.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Retail.Order.Data.Constants;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Mapper.PaymentUpdate
{
    public class CosmosUpdateMapper
    {
        public static CosmosModifyServiceCommand OrderUpdateServiceMapper(OrderDetails order)
        {
            CosmosDocumentEvent cosmosDocumentEvent = new()
            {
                Document = order,
                ModificationType = EAPI.Infrastructure.Entities.Enums.CosmosModificationType.Replace
            };

            return new CosmosModifyServiceCommand()
            {
                CollectionName = ConfigReader.Get(Environment.OrderCollection),
                DatabaseEndpoint = ConfigReader.Get(Environment.OrderDBEndpoint),
                DatabaseName = ConfigReader.Get(Environment.DatabaseName),
                Event = cosmosDocumentEvent
            };
        }

        public static CosmosModifyServiceCommand PnrSearchUpdateServiceMapper(PnrSearch pnrSearch)
        {
            CosmosDocumentEvent cosmosDocumentEvent = new()
            {
                Document = pnrSearch,
                ModificationType = EAPI.Infrastructure.Entities.Enums.CosmosModificationType.Replace
            };

            return new CosmosModifyServiceCommand()
            {
                CollectionName = ConfigReader.Get(Environment.OrderCollection),
                DatabaseEndpoint = ConfigReader.Get(Environment.OrderDBEndpoint),
                DatabaseName = ConfigReader.Get(Environment.DatabaseName),
                Event = cosmosDocumentEvent
            };
        }

    }
}
