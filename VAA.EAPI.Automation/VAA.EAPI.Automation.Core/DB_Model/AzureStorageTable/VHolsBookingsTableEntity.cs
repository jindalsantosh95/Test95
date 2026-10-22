using Microsoft.Azure.Cosmos.Table;

namespace VAA.EAPI.Automation.Core.DB_Model.AzureStorageTable
{
    public class VHolsBookingsTableEntity : TableEntity
    {
        public int? Id { get; set; }
        public string Client { get; set; }
        public string LeadName { get; set; }
        public string DateOfBirth { get; set; }
        public string Destination { get; set; }
        public string DepartureDate { get; set; }
        public string Inbound { get; set; }
        public string Items { get; set; }
        public string OptionStatus { get; set; }
        public string BookedUser { get; set; }
        public string BookingDate { get; set; }
        public string BookingStatus { get; set; }
        public string Brand { get; set; }
        public string Company { get; set; }
        public string Distribution { get; set; }
        public string Media { get; set; }
        public string GenericType { get; set; }
        public double? Price { get; set; }
    }
}
