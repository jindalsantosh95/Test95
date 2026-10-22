namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.RuleEngine
{
    public class SegmentResponse
    {
        public string AirlineCodeMarketing { get; set; }

        public string AirlineCodeOperating { get; set; }

        public string SegmentClass { get; set; }

        public string FlightNumberOperating { get; set; }

        public string FlightDate { get; set; }

        public string Origin { get; set; }

        public string Destination { get; set; }

        public string ProcessingComments { get; set; }

        public int PointsAward { get; set; }

        public int BasePoints { get; set; }

        public int CabinBonusPoints { get; set; }

        public int TierBonusPoints { get; set; }

        public int TierPointsAward { get; set; }
    }
}
