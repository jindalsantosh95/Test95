using System.ComponentModel.DataAnnotations;
using Microsoft.Azure.Cosmos.Table;

namespace VAA.EAPI.Automation.Core.DB_Model.AzureStorageTable
{
    public class SSRTableEntity : TableEntity
    {
        [Display(Order = 1)]
        public string PNR { get; set; }
        [Display(Order = 2)]
        public string PassengerResourceId { get; set; }
        [Display(Order = 3)]
        public string SSRCode { get; set; }
        [Display(Order = 4)]
        public string SSRName { get; set; }
        [Display(Order = 5)]
        public bool IsSelected { get; set; }
        [Display(Order = 6)]
        public string FlightNumber { get; set; }
        [Display(Order = 7)]
        public string Origin { get; set; }
        [Display(Order = 8)]
        public string Destination { get; set; }
        [Display(Order = 9)]
        public string GivenNames { get; set; }
        [Display(Order = 10)]
        public string Surname { get; set; }
    }
}
