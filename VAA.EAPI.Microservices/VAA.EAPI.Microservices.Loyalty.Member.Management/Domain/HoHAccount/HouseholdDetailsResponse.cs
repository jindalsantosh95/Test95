using System;
using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.HoHAccount
{
    public class HouseholdDetailsResponse
    {
        public List<HouseholdMembers> HouseholdMembers { get; set; }
        public HeadOfHousehold HeadOfHousehold { get; set; }
        public List<HouseholdHistory> HouseholdHistory { get; set; }
    }

    public class HouseholdMembers
    {
        public string CustomerId { get; set; }
        public DateTime? AssociationDate { get; set; }
        public string GroupDocId { get; set; }
        public string RelationshipType { get; set; }
        public string RetainPointTotal { get; set; }
        public string RollingPointTotal { get; set; }
        public string LastUpdatedBy { get; set; }
        public DateTime? LastUpdatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
    }

    public class HeadOfHousehold
    {
        public string CustomerId { get; set; }
        public DateTime? AssociationDate { get; set; }
        public string GroupDocId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
    }

    public class HouseholdHistory
    {
        public string CustomerId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string RelationshipType { get; set; }
        public string GroupDocId { get; set; }
        public string LastUpdatedBy { get; set; }
        public DateTime? LastUpdatedOn { get; set; }
        public string CreatedBy { get; set; }
    }
}
