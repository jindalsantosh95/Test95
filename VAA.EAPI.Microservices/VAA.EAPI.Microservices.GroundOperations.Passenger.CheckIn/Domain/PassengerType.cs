namespace VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Domain
{
    public class PassengerType
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }

        public static explicit operator PassengerType(DeltaSsci.Passenger1 source)
        {
            return new PassengerType()
            {
                Code = source.PsgrTypCde
            };
        }
    }
}
