using System;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models.PartnerDataManagement
{
    public class VSClaimDetails
    {
        public string Id { get; set; }

        public DateTime? ActivityDate { get; set; }

        public string FlightNumber { get; set; }

        public string OriginationAirport { get; set; }

        public string DestinationAirport { get; set; }

        public string RevenueClass { get; set; }

        public int PointsEarned { get; set; }

        public int TierPointsEarned { get; set; }

        public string IataAgentNumber { get; set; }

        public string SalesLocation { get; set; }

        public string RecordLocator { get; set; }

        public string VADealCode { get; set; }

        public bool Accepted { get; set; }

        public string Reason { get; set; }
    }
}
