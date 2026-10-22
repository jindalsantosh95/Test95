using Newtonsoft.Json;

namespace VAA.EAPI.Microservices.Retail.Order.ChangeFeedProcessor.Domain
{
    public partial class SegmentDetail
    {
        [JsonProperty("ArrivalTime")]
        public string? ArrivalTime { get; set; }

        [JsonProperty("DepartureTime")]
        public string? DepartureTime { get; set; }

        [JsonProperty("DepartureDate")]
        public string? DepartureDate { get; set; }

        [JsonProperty("DepartureYear")]
        public string? DepartureYear { get; set; }

        [JsonProperty("ArrivalPoint")]
        public string? ArrivalPoint { get; set; }

        [JsonProperty("CassEquipment")]
        public string? CassEquipment { get; set; }

        [JsonProperty("DepartureDayOfWeekNumber")]
        public string? DepartureDayOfWeekNumber { get; set; }

        [JsonProperty("DeparturePoint")]
        public string? DeparturePoint { get; set; }

        [JsonProperty("ElapsedTime")]
        public string? ElapsedTime { get; set; }

        [JsonProperty("FirstLegDepartureDate")]
        public string? FirstLegDepartureDate { get; set; }

        [JsonProperty("FirstLegDepartureCity")]
        public string? FirstLegDepartureCity { get; set; }

        [JsonProperty("FirstLegDepartureTime")]
        public string? FirstLegDepartureTime { get; set; }

        [JsonProperty("FirstLegDepartureYear")]
        public string? FirstLegDepartureYear { get; set; }

        [JsonProperty("FlightDepartureDate")]
        public string? FlightDepartureDate { get; set; }

        [JsonProperty("FlightLegDepartureYear")]
        public string? FlightLegDepartureYear { get; set; }

        [JsonProperty("FlightMileage")]
        public string? FlightMileage { get; set; }

        [JsonProperty("InventoryEquipment")]
        public string? InventoryEquipment { get; set; }

        [JsonProperty("LegArrivalDate")]
        public string? LegArrivalDate { get; set; }

        [JsonProperty("LegNumber")]
        public string? LegNumber { get; set; }

        [JsonProperty("MealServiceCode")]
        public string? MealServiceCode { get; set; }

        [JsonProperty("OfficialAirlineGuideEquipmentCode")]
        public string? OfficialAirlineGuideEquipmentCode { get; set; }

        [JsonProperty("SecureFlightFlag")]
        public string? SecureFlightFlag { get; set; }

        [JsonProperty("SegmentNumber")]
        public string? SegmentNumber { get; set; }

        [JsonProperty("ActiveFlag")]
        public string? ActiveFlag { get; set; }

        [JsonProperty("AirlineCode")]
        public string? AirlineCode { get; set; }

        [JsonProperty("BinaryFareTypeCode")]
        public string? BinaryFareTypeCode { get; set; }

        [JsonProperty("ClassOfService")]
        public string? ClassOfService { get; set; }

        [JsonProperty("ClassOfService_DualRBD")]
        public string? ClassOfService_DualRBD { get; set; }

        [JsonProperty("ClassOfServiceOverBookedFlag")]
        public string? ClassOfServiceOverBookedFlag { get; set; }

        [JsonProperty("CodeShareParticipantFlag")]
        public string? CodeShareParticipantFlag { get; set; }

        [JsonProperty("CompartmentNumber")]
        public string? CompartmentNumber { get; set; }

        [JsonProperty("CompartmentOverBookedFlag")]
        public string? CompartmentOverBookedFlag { get; set; }

        [JsonProperty("ContinuingFlag")]
        public string? ContinuingFlag { get; set; }

        [JsonProperty("CurrentActionCode")]
        public string? CurrentActionCode { get; set; }

        [JsonProperty("DepartureDateAdjustment")]
        public string? DepartureDateAdjustment { get; set; }

        [JsonProperty("ElectronicTicketEligibleFlag")]
        public string? ElectronicTicketEligibleFlag { get; set; }

        [JsonProperty("FirstIndexableFlag")]
        public string? FirstIndexableFlag { get; set; }

        [JsonProperty("FlightNumber")]
        public string? FlightNumber { get; set; }

        [JsonProperty("InFlightDateAdjustment")]
        public string? InFlightDateAdjustment { get; set; }

        [JsonProperty("MarriedControlAvilBucket")]
        public string? MarriedControlAvilBucket { get; set; }

        [JsonProperty("NoSmokingFlag")]
        public string? NoSmokingFlag { get; set; }

        [JsonProperty("NumberInParty")]
        public string? NumberInParty { get; set; }

        [JsonProperty("PoolAllocationFlag")]
        public string? PoolAllocationFlag { get; set; }

        [JsonProperty("PreviousActionCode")]
        public string? PreviousActionCode { get; set; }

        [JsonProperty("PseudoTripNumber")]
        public string? PseudoTripNumber { get; set; }

        [JsonProperty("RmsFareValue")]
        public string? RmsFareValue { get; set; }

        [JsonProperty("ScheduleChangeFlag")]
        public string? ScheduleChangeFlag { get; set; }

        [JsonProperty("SegmentDepartureYear")]
        public string? SegmentDepartureYear { get; set; }

        [JsonProperty("SegmentType")]
        public string? SegmentType { get; set; }

        [JsonProperty("UniqueBasId")]
        public string? UniqueBasId { get; set; }

        [JsonProperty("VitualInvBuckets")]
        public string? VitualInvBuckets { get; set; }

        [JsonProperty("MarketingCarrier")]
        public string? MarketingCarrier { get; set; }

        [JsonProperty("MarketingFlightNumber")]
        public string? MarketingFlightNumber { get; set; }

        [JsonProperty("OperatingCarrier")]
        public string? OperatingCarrier { get; set; }

        [JsonProperty("OperatingFlightNumber")]
        public string? OperatingFlightNumber { get; set; }

        [JsonProperty("OriginAirportCode")]
        public string? OriginAirportCode { get; set; }

        [JsonProperty("DestinationAirportCode")]
        public string? DestinationAirportCode { get; set; }

        [JsonProperty("FirstNameNumber")]
        public string? FirstNameNumber { get; set; }

        [JsonProperty("LastNameNumber")]
        public string? LastNameNumber { get; set; }

        [JsonProperty("OutboundConnectionFlag")]
        public string? OutboundConnectionFlag { get; set; }

        [JsonProperty("InboundConnectionFlag")]
        public string? InboundConnectionFlag { get; set; }

        [JsonProperty("ScheduledDepartureTime")]
        public ScheduledDepartureTime? ScheduledDepartureTime { get; set; }

        [JsonProperty("ScheduledArrivalTime")]
        public ScheduledArrivalTime? ScheduledArrivalTime { get; set; }

        [JsonProperty("ConnectionTime")]
        public string? ConnectionTime { get; set; }

        [JsonProperty("NumberOfStops")]
        public string? NumberOfStops { get; set; }

        [JsonProperty("FlightDistance")]
        public FlightDistance? FlightDistance { get; set; }

        [JsonProperty("FlightDuration")]
        public string? FlightDuration { get; set; }

        [JsonProperty("NextDayIndicator")]
        public string? NextDayIndicator { get; set; }

        [JsonProperty("Legs")]
        public IEnumerable<Legs>? Legs { get; set; }

        [JsonProperty("ProductId")]
        public string? ProductId { get; set; }

        [JsonProperty("SpaceSoldFromInventoryThisTransaction")]
        public string? SpaceSoldFromInventoryThisTransaction { get; set; }

        [JsonProperty("OandDTripNumberSegment")]
        public string? OandDTripNumberSegment { get; set; }

        [JsonProperty("SpaceFromInventory")]
        public string? SpaceFromInventory { get; set; }

        [JsonProperty("ReducedNumberInPartyOA")]
        public string? ReducedNumberInPartyOA { get; set; }

        [JsonProperty("ReducedNumberInPartyDL")]
        public string? ReducedNumberInPartyDL { get; set; }
    }
}
