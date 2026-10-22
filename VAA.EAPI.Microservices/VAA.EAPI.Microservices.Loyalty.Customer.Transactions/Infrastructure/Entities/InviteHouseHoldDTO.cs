using System.Collections.Generic;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.InviteHouseHold;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities
{
    public class InviteHouseHoldDTO : CosmosDocument
    {
        public string CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public string DocType { get; set; }
        public string LastUpdatedBy { get; set; }
        public string LastUpdatedOn { get; set; }
        public string GroupName { get; set; }
        public string GroupTypeCode { get; set; }
        public string Status { get; set; }
        public string InviterMemberNumber { get; set; }
        public string InviteeMemberNumber { get; set; }

        public static explicit operator InviteHouseHoldDTO(Inviter Inviter) => new()
        {
            Id = Inviter.Id,
            PartitionKey = Constants.Application.HouseholdGroupTypeCode,
            CreatedOn = Inviter.CreatedOn?.ToString(Constants.Application.ISODateTimeFormat),
            CreatedBy = Inviter.CreatedBy ?? string.Empty,
            LastUpdatedBy = Inviter.LastUpdatedBy,
            LastUpdatedOn = Inviter.LastUpdatedOn?.ToString(Constants.Application.ISODateTimeFormat),
            GroupName = Inviter.GroupName,
            Status = Inviter.Status,
            DocType = Constants.Application.HouseholdGroupTypeCode,
            InviterMemberNumber = Inviter.InviterMemberNumber,
            InviteeMemberNumber = Inviter.InviteeMemberNumber
        };
    }
}
