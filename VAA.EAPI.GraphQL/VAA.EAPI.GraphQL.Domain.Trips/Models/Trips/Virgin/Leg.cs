using HotChocolate;
using NodaTime;
using VAA.EAPI.GraphQL.Domain.Trips.Helper;
using VAA.EAPI.GraphQL.Domain.Trips.Models.ReferenceData;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin
{
    [GraphQLName("TripsLeg")]
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
        public DateTime? ScheduledDepartureUtc { get; set; }
        public DateTime? ScheduledArrivalUtc { get; set; }
        public DateTime? EstimatedDepartureUtc { get; set; }
        public DateTime? EstimatedArrivalUtc { get; set; }
        public DateTime? ActualDepartureUtc { get; set; }
        public DateTime? ActualArrivalUtc { get; set; }
        public LocalDateTime? ScheduledDepartureLocal { get; set; }
        public LocalDateTime? ScheduledArrivalLocal { get; set; }
        public LocalDateTime? ScheduledArrivalBasedOnDepartureLocal { get; set; }
        public LocalDateTime? EstimatedDepartureLocal { get; set; }
        public LocalDateTime? EstimatedArrivalLocal { get; set; }
        public LocalDateTime? ActualDepartureLocal { get; set; }
        public LocalDateTime? ActualArrivalLocal { get; set; }
        public AirportDetails? Origin { get; set; }
        public AirportDetails? Destination { get; set; }
        public string? OnAirDuration { get; set; }
        public string? Status { get; set; }
        public CabinClass? CabinClass { get; set; }
        public Aircraft? Aircraft { get; set; }
        public bool? IsStarlinkEligible { get; set; }
        public IEnumerable<SeatAssignment>? SeatAssignments { get; set; }
        public string? OnGroundDuration { get; set; }

        public static explicit operator Leg((Infrastructure.Models.TripMS.Leg leg, IEnumerable<CabinRefData> cabinDetails, IEnumerable<AirlineRefData> airlineDetails) source)
        {
            var (leg, cabinDetails, airlineDetails) = source;

            if (leg == null) return null;

            return new Leg()
            {
                Aircraft = (Aircraft)leg.Aircraft,
                IsStarlinkEligible = leg.StarlinkEligible,
                CabinClass = (CabinClass)(leg.CabinClass, cabinDetails),
                Destination = (AirportDetails)leg.Destination,
                Id = leg.Id,
                Marketing = (CarrierDetails)(leg.Marketing, airlineDetails),
                OnAirDuration = leg.OnAirDuration,
                Origin = (AirportDetails)leg.Origin,
                Operating = (CarrierDetails)(leg.Operating, airlineDetails),
                ScheduledArrivalBasedOnDepartureLocalDateTime = leg.ScheduledArrivalBasedOnDepartureLocalDateTime,
                ScheduledArrivalLocalDateTime = leg.ScheduledArrivalLocalDateTime,
                ScheduledArrivalUtcDateTime = leg.ScheduledArrivalUtcDateTime,
                ScheduledDepartureLocalDateTime = leg.ScheduledDepartureLocalDateTime,
                ScheduledDepartureUtcDateTime = leg.ScheduledDepartureUtcDateTime,
                EstimatedDepartureLocalDateTime = leg.EstimatedDepartureLocalDateTime,
                EstimatedDepartureUtcDateTime = leg.EstimatedDepartureUtcDateTime,
                EstimatedArrivalLocalDateTime = leg.EstimatedArrivalLocalDateTime,
                EstimatedArrivalUtcDateTime = leg.EstimatedArrivalUtcDateTime,
                ActualDepartureLocalDateTime = leg.ActualDepartureLocalDateTime,
                ActualDepartureUtcDateTime = leg.ActualDepartureUtcDateTime,
                ActualArrivalLocalDateTime = leg.ActualArrivalLocalDateTime,
                ActualArrivalUtcDateTime = leg.ActualArrivalUtcDateTime,
                SeatAssignments = leg.SeatAssignments?.Select(x => (SeatAssignment)(x, cabinDetails)),
                Status = leg.Status,
                OnGroundDuration = leg.OnGroundDuration,
                ScheduledDepartureUtc = leg.ScheduledDepartureUtcDateTime,
                ScheduledArrivalUtc = leg.ScheduledArrivalUtcDateTime,
                EstimatedDepartureUtc = leg.EstimatedDepartureUtcDateTime,
                EstimatedArrivalUtc = leg.EstimatedArrivalUtcDateTime,
                ActualDepartureUtc = leg.ActualDepartureUtcDateTime,
                ActualArrivalUtc = leg.ActualArrivalUtcDateTime,
                ScheduledDepartureLocal = DateTimeHelper.ToLocal(leg.ScheduledDepartureLocalDateTime),
                ScheduledArrivalLocal = DateTimeHelper.ToLocal(leg.ScheduledArrivalLocalDateTime),
                ScheduledArrivalBasedOnDepartureLocal = DateTimeHelper.ToLocal(leg.ScheduledArrivalBasedOnDepartureLocalDateTime),
                EstimatedDepartureLocal = DateTimeHelper.ToLocal(leg.EstimatedDepartureLocalDateTime),
                EstimatedArrivalLocal = DateTimeHelper.ToLocal(leg.EstimatedArrivalLocalDateTime),
                ActualDepartureLocal = DateTimeHelper.ToLocal(leg.ActualDepartureLocalDateTime),
                ActualArrivalLocal = DateTimeHelper.ToLocal(leg.ActualArrivalLocalDateTime)
            };
        }

        public static explicit operator Leg((Infrastructure.Models.HolidaysMS.TripsCore.Leg leg, IEnumerable<AirportRefData> airportRefData) source)
            => (Leg)(source.leg, source.airportRefData, Enumerable.Empty<AirlineRefData>());

        public static explicit operator Leg((Infrastructure.Models.HolidaysMS.TripsCore.Leg leg, IEnumerable<AirportRefData> airportRefData, IEnumerable<AirlineRefData> airlineRefData) source)
        {
            var (leg, airportRefData, airlineRefData) = source;

            if (leg == null) return null;

            string status = leg.Status;
            var airportData = airportRefData?.FirstOrDefault(x => x.AirportCode == leg.Destination?.Code);
            var timeZoneInfo = DateTimeHelper.TryFindSystemTimeZoneById(airportData?.TimeZone);
            if (timeZoneInfo != null)
            {
                var scheduleArrivalUtc = leg.ScheduledArrivalLocal.HasValue ? TimeZoneInfo.ConvertTimeToUtc(leg.ScheduledArrivalLocal.Value, timeZoneInfo) : (DateTime?)null;
                if (scheduleArrivalUtc.HasValue && scheduleArrivalUtc.Value < DateTime.UtcNow)
                {
                    status = "FLIGHT_COMPLETE";
                }
            }

            return new Leg()
            {
                Aircraft = (Aircraft)leg.Aircraft,
                Destination = (AirportDetails)(leg.Destination, airportRefData),
                Id = leg.Id,
                Marketing = (CarrierDetails)(leg.Marketing, airlineRefData),
                OnAirDuration = leg.OnAirDuration,
                Origin = (AirportDetails)(leg.Origin, airportRefData),
                Operating = (CarrierDetails)(leg.Operating, airlineRefData),
                Status = status,
                OnGroundDuration = leg.OnGroundDuration,
                ScheduledDepartureLocal = DateTimeHelper.ToLocal(leg.ScheduledDepartureLocal),
                ScheduledArrivalLocal = DateTimeHelper.ToLocal(leg.ScheduledArrivalLocal),
            };
        }
    }
}
