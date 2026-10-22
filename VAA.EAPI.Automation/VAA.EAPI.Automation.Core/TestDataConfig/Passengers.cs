namespace VAA.EAPI.Automation.Core.TestDataConfig
{
    public class Passengers
    {
        public List<Passenger> Adults { get; set; }

        public List<Passenger> YoungAdults { get; set; }

        public List<Passenger> Children { get; set; }

        public List<Passenger> Infants { get; set; }

        public Passengers()
        {
            Adults = [];
            YoungAdults = [];
            Children = [];
            Infants = [];
        }
    }
}