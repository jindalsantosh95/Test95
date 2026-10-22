using System;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models.Transaction
{
    public class VredStatement
    {
        public string Id { get; set; }
        public DateTime ActivityDate { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string PointsInt { get; set; }
        public VredTransactionDetail VredDetail { get; set; }
    }
}
