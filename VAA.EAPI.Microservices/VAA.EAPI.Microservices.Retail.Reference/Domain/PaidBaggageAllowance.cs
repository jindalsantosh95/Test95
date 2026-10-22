namespace VAA.EAPI.Microservices.Retail.Reference.Domain
{
    public class PaidBaggageAllowance : FreeBaggageAllowance
    {
        public double FirstBagCost { get; set; }
        public double SecondBagCost { get; set; }
        public string Currency { get; set; }
    }
}
