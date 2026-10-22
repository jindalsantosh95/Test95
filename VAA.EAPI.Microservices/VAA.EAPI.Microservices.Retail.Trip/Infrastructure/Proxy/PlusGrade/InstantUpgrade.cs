namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.PlusGrade
{
    public class InstantUpgrade
    {
        public bool Eligible { get; set; }
        public string IneligibleReason { get; set; }
        public string Amount { get; set; }
        public string PointsAmount { get; set; }
    }
}

