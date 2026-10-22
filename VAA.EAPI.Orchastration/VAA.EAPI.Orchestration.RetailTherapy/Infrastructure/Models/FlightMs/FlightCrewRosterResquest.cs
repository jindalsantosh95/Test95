using System;

namespace VAA.EAPI.Orchestration.RetailTherapy.Infrastructure.Models.FlightMs
{
    public sealed class FlightCrewRosterRequest
    {
        public DateOnly FlightDate { get; set; }
        public string Departure { get; set; } = string.Empty;
        public int FlightNo { get; set; } = 0;
    }
}
