using System;

namespace VAA.EAPI.Orchestration.Notifications.Infrastructure.Models.FlightMs
{
    public sealed class SeatSelectionReminderFlightDetail
    {
        public string Id => $"{Carrier}{Date:yyyyMMdd}{ScheduledTimeOfDeparture:HHmm}{FlightNo}";
        public string Carrier { get; set; }
        public int FlightNo { get; set; }
        public TimeOnly ScheduledTimeOfDeparture { get; set; }
        public DateOnly Date { get; set; }
        public string Departure { get; set; }
        public string Arrival { get; set; }
        public string Registration { get; set; }
        public DateTime ScheduledDateTimeOfDeparture => Date.ToDateTime(ScheduledTimeOfDeparture);

        public static explicit operator SeatSelectionReminderFlightDetail(FlightDetail source)
        {
            return new SeatSelectionReminderFlightDetail
            {
                Carrier = source.Carrier,
                FlightNo = source.FlightNo,
                ScheduledTimeOfDeparture = source.ScheduledTimeOfDeparture,
                Date = source.Date,
                Departure = source.Departure,
                Arrival = source.Arrival,
                Registration = source.Registration
            };
        }
    }
}
