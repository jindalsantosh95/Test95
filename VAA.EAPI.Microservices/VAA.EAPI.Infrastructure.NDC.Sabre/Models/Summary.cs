using System;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Models
{
    public class Summary
    {
        public string DocumentNumber { get; set; }
        public DateTime LocalIssueDateTime { get; set; }
        public string DocumentType { get; set; }
        public string IssuingLocation { get; set; }
        public Reservation Reservation { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public TotalAmount TotalAmount { get; set; }
    }


}
