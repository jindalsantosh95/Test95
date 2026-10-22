using Microsoft.Azure.Cosmos.Table;

namespace VAA.EAPI.Automation.Core.DB_Model.AzureStorageTable
{
    public class PassportDataTableEntity : TableEntity
    {
        public string PassportNumber { get; set; }
    }
}