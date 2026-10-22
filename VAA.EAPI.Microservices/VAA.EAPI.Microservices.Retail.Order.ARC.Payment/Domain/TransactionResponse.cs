namespace VAA.EAPI.Microservices.Retail.Order.ARC.Payment.Domain
{
    public class TransactionResponse
    {
        public string AgentNumericCode { get; set; }
        public string AccreditationType { get; set; }
        public Arc Arc { get; set; }
        public IEnumerable<Ticketing> Ticketing { get; set; }
    }
}
