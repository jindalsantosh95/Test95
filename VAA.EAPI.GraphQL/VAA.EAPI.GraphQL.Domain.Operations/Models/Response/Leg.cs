namespace VAA.EAPI.GraphQL.Domain.Operations.Models.Response
{
    public class Leg
    {
        public string Leg_Id { get; set; }
        public string Origin_Airport_Code { get; set; }
        public string Destination_Airport_Code { get; set; }
        public MovementTime Scheduled_Departure_Time { get; set; }
        public MovementTime Scheduled_Arrival_Time { get; set; }
        public MovementTime Actual_Departure_Time { get; set; }
        public MovementTime Actual_Arrival_Time { get; set; }
        public MovementTime Estimated_Departure_Time { get; set; }
        public MovementTime Estimated_Arrival_Time { get; set; }
        public MovementTime Wheels_Up { get; set; }
        public MovementTime Wheels_Down { get; set; }
        public string Departure_Status { get; set; }
        public string Arrival_Status { get; set; }
        public string Flight_Cancelled { get; set; }
        public IEnumerable<Compartment> Compartments { get; set; }
        public string Wheels_Up_Status { get; set; }
        public string Wheels_Down_Status { get; set; }
    }

}
