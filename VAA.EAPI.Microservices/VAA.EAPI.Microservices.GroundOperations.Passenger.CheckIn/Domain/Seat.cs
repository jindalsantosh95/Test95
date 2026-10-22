namespace VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Domain
{
    public class Seat
    {
        public int Row { get; set; }
        public string Column { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public IEnumerable<ActualCabinClass> ActualCabinClass { get; set; }
    }
}