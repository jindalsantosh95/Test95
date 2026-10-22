using Microsoft.Azure.Cosmos.Table;

namespace VAA.EAPI.Automation.Core.DB_Model.AzureStorageTable
{
    public class CreatedPnrsTableEntity : TableEntity
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PaymentCardType { get; set; }
        public string CardNumber { get; set; }
        public string PRI { get; set; }
        public string ChildPRI { get; set; }
        public string OrderNo { get; set; }
        public string Airline { get; set; }
        public string FlightNumber { get; set; }
        public string Journey { get; set; }
        public string ThreeDS { get; set; }
        public string Env { get; set; }
        public string Origin { get; set; }
        public string Destination { get; set; }
        public string TotalPrice { get; set; }
        public string Email { get; set; }
        public string EmailFromBasket { get; set; }
        public string Currency { get; set; }
        public string OutBoundCabinClass { get; set; }
        public string InBoundCabinClass { get; set; }
        public string DepartureDate { get; set; }
        public string FlightJourney { get; set; }
        public bool? IsDirect { get; set; }
        public bool? IsCodeShare { get; set; }
        public bool? IsSeats { get; set; }
        public string SeatUpdate { get; set; }
        public double? InsurancePrice { get; set; }
        public string Passengers { get; set; }
        public string PassengersCount { get; set; }
        public string FlightType { get; set; }
        public string FlyingClubNumber { get; set; }
        public int? FCPointsBeforeRedeem { get; set; }
        public int? FCPointsAfterRedeem { get; set; }
        public int? FlightPoints { get; set; }
        public string TierCode { get; set; }
        public string CabinClass { get; set; }
        public string FQTProgram { get; set; }
        public string CheckInStatus { get; set; }
        public string SeatConfirmationResponse { get; set; }
        public string ReservationType { get; set; }
        public string BaggageSelectionType { get; set; }
    }
}