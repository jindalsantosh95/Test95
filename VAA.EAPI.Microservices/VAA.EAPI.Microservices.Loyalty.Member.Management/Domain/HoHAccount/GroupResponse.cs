using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.HoHAccount
{
    public class GroupResponse
    {
        public IEnumerable<HouseHoldGroup> GroupDocuments { get; set; }
    }
}
