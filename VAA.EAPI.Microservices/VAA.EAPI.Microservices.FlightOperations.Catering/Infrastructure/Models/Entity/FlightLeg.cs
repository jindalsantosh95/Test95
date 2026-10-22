using System;

namespace VAA.EAPI.Microservices.FlightOperations.Catering.Infrastructure.Models.Entity
{
    public class FlightLeg
    {  
        public int Leg { get; set; } 
        public int FlightId { get; set; }
        public DateTime? FlightDate { get; set; }
        public int? LegSequenceNumber => SeqNo; 
        public int? SeqNo { get; set; }
        public string Origin { get; set; }
        public string Destination { get; set; }
        public int? Aircraft { get; set; }
        public string AircraftRegistration { get; set; }
        public DateTime ScheDepTimestampLoc { get; set; }
        public DateTime ScheDepTimestampUtc { get; set; }
        public DateTime? DepTimestampLoc { get; set; }
        public DateTime? DepTimestampUtc { get; set; }
        public DateTime ScheArrTimestampLoc { get; set; }
        public DateTime ScheArrTimestampUtc { get; set; }
        public DateTime? ArrTimestampLoc { get; set; }
        public DateTime? ArrTimestampUtc { get; set; }
        public string Status { get; set; }
        public string OriginTerminal { get; set; }
        public string DestinationTerminal { get; set; }
        public int? TotalPaxCapacity { get; set; }
        public int? NoPaxBooked { get; set; }
        public int? NoStaffPax { get; set; }
        public int? NoStaffCompanionPax { get; set; }
        public int? NoPaxWithConnectFlight { get; set; }
        public int? NoDeportee { get; set; }
        public int? NoUnaccompaniedMinor { get; set; }
        public int? NoPreferentialPax { get; set; }
        public int? NoInfantPax { get; set; }
        public string Priority { get; set; }

        // Navigation
        public Flight Flight { get; set; }
    }

}
