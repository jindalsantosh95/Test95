public class MemberTierHistory
{
    public string code { get; set; }
    public string message { get; set; }
    public IEnumerable<TierHistoryItem> tierHistoryItems { get; set; }
}

public class TierHistoryItem
{
    public string tier { get; set; }
    public string tierDocId { get; set; }
    public DateTime? startDate { get; set; }
    public DateTime? endDate { get; set; }
    public int? qualifyingTierPoints { get; set; }
    public string active { get; set; }
    public string issueReason { get; set; }
    public string userCreated { get; set; }
    public string downGradeProhibitReason { get; set; }
    public DateTime? approveRejectDate { get; set; }
    public string approvalStatus { get; set; }
    public string revision { get; set; }
    public string downGradeProhibitFlag { get; set; }
    public string userUpdated { get; set; }
    public DateTime? updatedOn { get; set; }
    public string appliedRuleType { get; set; }
}

