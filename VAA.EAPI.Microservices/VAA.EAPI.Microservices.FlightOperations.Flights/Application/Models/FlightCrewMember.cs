using VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.Aims.Response;

namespace VAA.EAPI.Microservices.FlightOperations.Flights.Application.Models
{
    public sealed class FlightCrewMember
    {
        public static explicit operator FlightCrewMember(LegMembsTAIMSGetLegMembersTAIMSMember source) => new()
        {
            Id = source.id,
            PayrollNumber = source.PayrollNumber,
            Ac = source.ac,
            Base = source.@base,
            Crsday = source.crsday,
            Crsdaydd = source.crsdaydd,
            Crsdaymm = source.crsdaymm,
            Crsdayyy = source.crsdayyy,
            Crte = source.crte,
            FirstName = source.firstName,
            LastName = source.lastName,
            Name = source.name,
            Notif = source.notif,
            Pax = source.pax,
            Pos = source.pos,
            Qualcat = source.qualcat,
            Request = source.request,
            ShortName = source.shortName,
            Trnduty = source.trnduty,
        };

        public int Id { get; set; }
        public string PayrollNumber { get; set; }
        public string Ac { get; set; }
        public string Base { get; set; }
        public ushort Crsday { get; set; }
        public ushort Crsdaydd { get; set; }
        public ushort Crsdaymm { get; set; }
        public ushort Crsdayyy { get; set; }
        public string Crte { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Name { get; set; }
        public string Notif { get; set; }
        public string Pax { get; set; }
        public string Pos { get; set; }
        public string Qualcat { get; set; }
        public string Request { get; set; }
        public string ShortName { get; set; }
        public string Trnduty { get; set; }
    }
}
