
public class Data
{
    public LoyaltyMemberHousehold loyaltyMemberHousehold { get; set; }
}

public class HouseholdHistory
{
    public string customerId { get; set; }
    public DateTime? endDate { get; set; }
    public string groupDocId { get; set; }
    public string firstName { get; set; }
    public string lastName { get; set; }
    public string memberNumber { get; set; }
    public string memberStatus { get; set; }
    public string relationshipType { get; set; }
    public DateTime? startDate { get; set; }
    public string lastUpdatedBy { get; set; }
    public DateTime? lastUpdatedOn { get; set; }
    public string createdBy { get; set; }
}

public class HouseholdMember
{
    public DateTime? associationDate { get; set; }
    public string customerId { get; set; }
    public string firstName { get; set; }
    public string memberNumber { get; set; }
    public string memberStatus { get; set; }
    public string lastName { get; set; }
    public string groupDocId { get; set; }
    public string lastUpdatedBy { get; set; }
    public DateTime? lastUpdatedOn { get; set; }
    public string createdBy { get; set; }
    public DateTime? createdOn { get; set; }
}

public class LoyaltyMemberHousehold
{
    public HeadOfHouseHold headOfHousehold { get; set; }
    public List<HouseholdHistory> householdHistory { get; set; }
    public List<HouseholdMember> householdMembers { get; set; }
}

public class HeadOfHouseHold
{
    public string memberNumber { get; set; }
    public string customerId { get; set; }
    public string firstName { get; set; }
    public string lastName { get; set; }
    public string memberStatus { get; set; }
    public DateTime? associationDate { get; set; }
    public string groupDocId { get; set; }
    public DateTime? createdOn { get; set; }
    public string createdBy { get; set; }
}

public class HouseHoldDetailsRetrieval
{
    public Data data { get; set; }
}

