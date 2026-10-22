namespace VAA.EAPI.GraphQL.Domain.Account.Models.Activity
{
    public class ActivityResponse
    {
        public IEnumerable<ActivityDetail> ActivityDetails { get; set; }
        public ErrorResponse ErrorResponse { get; set; }
    }
    public class ActivityDetail
    {
        public ActivityDetail()
        {
        }

        public ActivityDetail(ActivityDetail activity)
        {
            ArgumentNullException.ThrowIfNull(activity);

            ActivityDate = activity.ActivityDate;
            PartnerCode = activity.PartnerCode;
            PartnerName = activity.PartnerName;
            Statement = activity.Statement;
            TierPoints = activity.TierPoints;
            TransactionId = activity.TransactionId;
            TransactionProcessedDate = activity.TransactionProcessedDate;
            TransactionType = activity.TransactionType;
            VirginPoints = activity.VirginPoints;
        }

        public DateTime ActivityDate { get; set; }
        public DateTime TransactionProcessedDate { get; set; }
        public string PartnerName { get; set; }
        public string Statement { get; set; }
        public string VirginPoints { get; set; }
        public string TierPoints { get; set; }
        public string PartnerCode { get; set; }
        public string TransactionId { get; set; }
        public string TransactionType { get; set; }
    }
}
