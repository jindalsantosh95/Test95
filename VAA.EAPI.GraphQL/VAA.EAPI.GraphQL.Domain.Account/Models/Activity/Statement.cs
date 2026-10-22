namespace VAA.EAPI.GraphQL.Domain.Account.Models.Activity
{
    public class Statement
    {
        public string Id { get; set; }
        public DateTime ActivityDate { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string PointsInt { get; set; }
        public Detail VredDetail { get; set; }
    }
}
