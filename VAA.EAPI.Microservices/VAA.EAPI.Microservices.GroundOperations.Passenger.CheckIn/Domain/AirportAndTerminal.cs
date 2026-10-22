namespace VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Domain
{
    public class AirportAndTerminal
    {
        public string? TerminalName { get; set; }
        public string? Code { get; set; }
        public string? AirportName { get; set; }
        public string? CountryCode { get; set; }

        public static explicit operator AirportAndTerminal((DeltaTrm.Station station, string? terminalName) source)
        {
            return new AirportAndTerminal
            {
                TerminalName = source.terminalName,
                Code = source.station.Code,
                AirportName = source.station.Name,
                CountryCode = source.station.Country.IsoAlpha3Code
            };
        }
    }
}