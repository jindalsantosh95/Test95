using System;
using System.Text;
using VAA.EAPI.Common.Extensions;

namespace VAA.EAPI.Orchestration.RetailTherapy.Infrastructure.Models.FlightMs
{
    public sealed class FlightDetail
    {
        public string Carrier { get; set; }
        public int FlightNo { get; set; }
        public string Type { get; set; }
        public string Departure { get; set; }
        public string Arrival { get; set; }
        public TimeOnly ScheduledTimeOfDeparture { get; set; }
        public TimeOnly ScheduledTimeOfArrival { get; set; }
        public string AircraftType { get; set; }
        public string Registration { get; set; }
        public int Capacity { get; set; }
        public DateOnly Date { get; set; }
        public DateOnly EstimatedDateOfDeparture { get; set; }
        public DateOnly EstimatedDateOfArrival { get; set; }

        public override string ToString()
        {
            StringBuilder stringBuilder = new();

            stringBuilder.AppendJoin(';', $"{Date:yyyyMMdd}{ScheduledTimeOfDeparture:HHmm}{FlightNo}",
                                          Carrier.EscapeCsvValue(),
                                          FlightNo.EscapeCsvValue(),
                                          Departure.EscapeCsvValue(),
                                          Arrival.EscapeCsvValue(),
                                          $"{Date:yyyy-MM-dd}T{ScheduledTimeOfDeparture:HH:mm:ss}".EscapeCsvValue(),
                                          $"{EstimatedDateOfArrival:yyyy-MM-dd}T{ScheduledTimeOfArrival:HH:mm:ss}".EscapeCsvValue(),
                                          AircraftType.EscapeCsvValue(),
                                          Registration.EscapeCsvValue(),
                                          Capacity.EscapeCsvValue(),
                                          string.Empty);

            return stringBuilder.ToString();
        }
    }
}
