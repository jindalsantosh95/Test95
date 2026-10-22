using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaMyTrips;
using VAA.EAPI.Microservices.Retail.Trip.Mapper;

namespace VAA.EAPI.Microservices.Retail.Trip.Domain.TripDetails
{
    public class Leg
    {
        public string? Id { get; set; }
        public CarrierDetails? Operating { get; set; }
        public CarrierDetails? Marketing { get; set; }
        public DateTime? ScheduledDepartureLocalDateTime { get; set; }
        public DateTime? ScheduledDepartureUtcDateTime { get; set; }
        public DateTime? ScheduledArrivalLocalDateTime { get; set; }
        public DateTime? ScheduledArrivalUtcDateTime { get; set; }
        public DateTime? ScheduledArrivalBasedOnDepartureLocalDateTime { get; set; }
        public DateTime? EstimatedDepartureLocalDateTime { get; set; }
        public DateTime? EstimatedDepartureUtcDateTime { get; set; }
        public DateTime? EstimatedArrivalLocalDateTime { get; set; }
        public DateTime? EstimatedArrivalUtcDateTime { get; set; }
        public DateTime? ActualDepartureLocalDateTime { get; set; }
        public DateTime? ActualDepartureUtcDateTime { get; set; }
        public DateTime? ActualArrivalLocalDateTime { get; set; }
        public DateTime? ActualArrivalUtcDateTime { get; set; }
        public AirportDetails? Origin { get; set; }
        public AirportDetails? Destination { get; set; }
        public string? OnAirDuration { get; set; }
        public string? Status { get; set; }
        public CabinClass? CabinClass { get; set; }
        public Aircraft? Aircraft { get; set; }
        public IEnumerable<SeatAssignment>? SeatAssignments { get; set; }
        public string? OnGroundDuration { get; set; }
        public string? SeatSelectionType { get; set; }

        public static explicit operator Leg(PassengerLeg source)
        {
            return new Leg
            {
                Id = source.LegId.ToString(),
                SeatAssignments = !source.SeatAssignments.IsNullOrEmpty() ? source.SeatAssignments?.Select(a => (SeatAssignment)a) : null
            };
        }

        public static explicit operator Leg(Infrastructure.Proxy.DeltaMyTrips.Leg source)
        {
            return new Leg
            {
                Id = source.LegId.ToString(),
                CabinClass = (CabinClass)source.CabinClass,
                Aircraft = (Aircraft)source.TransportEquipment,
                Status = source.Status,
                OnAirDuration = source.OnAirDuration,
                Origin = (AirportDetails)source.TransportOrigin,
                Destination = (AirportDetails)source.TransportDestination,
                Operating = DeltaTripDetailsMapper.MapToLegOperatingCarrier(source.Carrier?.FirstOrDefault(), source.FlightNum, source.OperationalSuffix),
                ScheduledDepartureLocalDateTime = source.TransportOrigin?.ScheduledDepartureLocalDateTime,
                ScheduledDepartureUtcDateTime = source.TransportOrigin?.ScheduledDepartureUtcDateTime,
                ScheduledArrivalUtcDateTime = source.TransportDestination?.ScheduledArrivalUtcDateTime,
                ScheduledArrivalLocalDateTime = source.TransportDestination?.ScheduledArrivalLocalDateTime,
                ScheduledArrivalBasedOnDepartureLocalDateTime = source.TransportDestination?.ScheduledArrivalBasedOnDepartureLocalDateTime,
                EstimatedDepartureLocalDateTime = source.TransportOrigin?.EstimatedDepartureLocalDateTime,
                EstimatedDepartureUtcDateTime = source.TransportOrigin?.EstimatedDepartureUtcDateTime,
                EstimatedArrivalLocalDateTime = source.TransportDestination?.EstimatedArrivalLocalDateTime,
                EstimatedArrivalUtcDateTime = source.TransportDestination?.EstimatedArrivalUtcDateTime, 
                ActualDepartureLocalDateTime = source.TransportOrigin?.ActualDepartureLocalDateTime,
                ActualDepartureUtcDateTime = source.TransportOrigin?.ActualDepartureUtcDateTime,
                ActualArrivalLocalDateTime = source.TransportDestination?.ActualArrivalLocalDateTime,
                ActualArrivalUtcDateTime = source.TransportDestination?.ActualArrivalUtcDateTime,
                OnGroundDuration = source.OnGroundDuration,
                SeatSelectionType = source.SeatSelectionType,
            };
        }
    }
}