using System;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Proxy.SCVPlusProfile
{
    public partial class ChildMember
    {
        public ChildMember(string subordinateName = default, string status = default, DateTime? startDate = default, string subordinateCardNumber = default, DateTime? householdLeaveDate = default, string subordinateMemId = default, string subordinateContactId = default)
        {
            SubordinateName = subordinateName;
            Status = status;
            StartDate = startDate;
            SubordinateCardNumber = subordinateCardNumber;
            HouseholdLeaveDate = householdLeaveDate;
            SubordinateMemId = subordinateMemId;
            SubordinateContactId = subordinateContactId;
        }

        public string SubordinateName { get; set; }
        public string Status { get; set; }
        public DateTime? StartDate { get; set; }
        public string SubordinateCardNumber { get; set; }
        public DateTime? HouseholdLeaveDate { get; set; }
        public string SubordinateMemId { get; set; }
        public string SubordinateContactId { get; set; }
    }
}
