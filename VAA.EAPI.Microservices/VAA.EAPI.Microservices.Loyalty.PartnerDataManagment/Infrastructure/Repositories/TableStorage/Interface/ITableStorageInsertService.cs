using Microsoft.Azure.Cosmos.Table;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Repositories.TableStorage.Interface
{
    public interface ITableStorageInsertService
    {
        Task<bool> TryInsertAsync<T>(string tableName, T entity, CancellationToken cancellationToken) where T : TableEntity;
    }
}
