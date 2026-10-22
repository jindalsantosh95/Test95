using TableEntity = Microsoft.Azure.Cosmos.Table.TableEntity;

namespace VAA.EAPI.Automation.Core.DB_Model.AzureStorageTable
{
    public class LoyaltyVouchersTableEntity : TableEntity
    {
        public string Vouchers { get; set; } = string.Empty;

        public string TierCode { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public int Points { get; set; }

        public DateTime? VoucherCreatedDate { get; set; }

        public bool IsVoucherExpiryWithInSixMonths { get; set; }
    }

}
