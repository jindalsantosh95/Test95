using TableEntity = Microsoft.Azure.Cosmos.Table.TableEntity;

namespace VAA.EAPI.Automation.Core.DB_Model.AzureStorageTable
{
    public class LoyaltyHouseHoldandHighFiveAccountsTableEntity : TableEntity
    {
        public string Environment { get; set; }
        public string ParentFCNumber { get; set; }
        public string ParentFirstName { get; set; }
        public string ParentLastName { get; set; }
        public string ParentTierCode { get; set; }
        public string ChildFCNumbers { get; set; }          
        public string MemberNumber { get; set; }
        public string ActiveYears { get; set; }
    }
}