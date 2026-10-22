using Microsoft.Azure.Cosmos.Table;

namespace VAA.EAPI.Automation.Core.DB_Model.AzureStorageTable
{
    public class NotificationsTableEntity : TableEntity
    {
        public string PayLoad { get; set; }
    }
}