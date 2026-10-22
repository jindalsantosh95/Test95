
using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.FlightBookings.Entity
{
    public class SegmentDetail
    {
        public string ArrivalPoint { get; set; }
        public string ArrivalTime { get; set; }
        public string CassEquipment { get; set; }
        public string DepartureDayOfWeekNumber { get; set; }
        public string DeparturePoint { get; set; }
        public string DepartureTime { get; set; }
        public string ElapsedTime { get; set; }
        public string FirstLegDepartureCity { get; set; }
        public string FirstLegDepartureDate { get; set; }
        public string FirstLegDepartureTime { get; set; }
        public string FirstLegDepartureYear { get; set; }
        public string FlightDepartureDate { get; set; }
        public string FlightLegDepartureYear { get; set; }
        public string FlightMileage { get; set; }
        public string InventoryEquipment { get; set; }
        public string LegArrivalDate { get; set; }
        public string LegNumber { get; set; }
        public string MealServiceCode { get; set; }
        public string OfficialAirlineGuideEquipmentCode { get; set; }
        public string SecureFlightFlag { get; set; }
        public string SegmentNumber { get; set; }
        public string ActiveFlag { get; set; }
        public string AirlineCode { get; set; }
        public string BinaryFareTypeCode { get; set; }
        public string ClassOfService { get; set; }
        public string ClassOfServiceOverBookedFlag { get; set; }
        public string CodeShareParticipantFlag { get; set; }
        public string CompartmentNumber { get; set; }
        public string CompartmentOverBookedFlag { get; set; }
        public string ContinuingFlag { get; set; }
        public string CurrentActionCode { get; set; }
        public string DepartureDate { get; set; }
        public string DepartureDateAdjustment { get; set; }
        public string ElectronicTicketEligibleFlag { get; set; }
        public string FirstIndexableFlag { get; set; }
        public string FlightNumber { get; set; }
        public string InFlightDateAdjustment { get; set; }
        public string MarriedControlAvilBucket { get; set; }
        public string NoSmokingFlag { get; set; }
        public string NumberInParty { get; set; }
        public string PoolAllocationFlag { get; set; }
        public string PreviousActionCode { get; set; }
        public string PseudoTripNumber { get; set; }
        public string RmsFareValue { get; set; }
        public string ScheduleChangeFlag { get; set; }
        public string SegmentDepartureYear { get; set; }
        public string SegmentType { get; set; }
        public string UniqueBasId { get; set; }
        public string VitualInvBuckets { get; set; }
        public string MarketingCarrier { get; set; }
        public string MarketingFlightNumber { get; set; }
        public string OperatingCarrier { get; set; }
        public string OperatingFlightNumber { get; set; }
        public string OriginAirportCode { get; set; }
        public string DestinationAirportCode { get; set; }
        public string FirstNameNumber { get; set; }
        public string LastNameNumber { get; set; }
        public string OutboundConnectionFlag { get; set; }
        public string InboundConnectionFlag { get; set; }
        public ScheduledDepartureTime ScheduledDepartureTime { get; set; }
        public ScheduledArrivalTime ScheduledArrivalTime { get; set; }
        public string ConnectionTime { get; set; }
        public string NumberOfStops { get; set; }
        public FlightDistance FlightDistance { get; set; }
        public string FlightDuration { get; set; }
        public string NextDayIndicator { get; set; }
        public IEnumerable<Leg> Legs { get; set; }
        public string ProductId { get; set; }
    }
}
