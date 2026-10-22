namespace VAA.EAPI.Microservices.Loyalty.Vred.Entities.VredStatements
{
    public class Statement
    {
        public string Id { get; set; }
        public string ActivityDate { get; set; }
        public string TransactionDate { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string PointsInt { get; set; }
        public string Details { get; set; }
        public Detail VredDetail { get; set; }
    }
}