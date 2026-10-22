using TableEntity = Microsoft.Azure.Cosmos.Table.TableEntity;

namespace VAA.EAPI.Automation.Core.DB_Model.AzureStorageTable
{
    public class LoyaltyAccountsTableEntity : TableEntity
    {
        public string LoyaltyAccountNumber { get; set; }
        public string Environment { get; set; }
        public string Tier { get; set; }
        public int Points { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string DateOfBirth { get; set; }
        public bool? UnderSixteen { get; set; }
        public string CountryAddress { get; set; }
        public bool? CoBrand { get; set; }
        public DateTime? CreatedDate { get; set; }
        public bool? IsWingsMember { get; set; }
        public bool ForHouseHold { get; set; }
    }
}
