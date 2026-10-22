namespace VAA.EAPI.Microservices.CRM.Notification.Domain.Entities.AircraftChangeNotification
{
    public class AircraftChangeRequest
    {
        public string LeadPaxFirstName { get; set; }
        public string LeadPaxLastName { get; set; }
        public string BookingReference { get; set; }
        public string FlightNumber { get; set; }
        public string Origin { get; set; }
        public string Destination { get; set; }
        public string DepartureMonth { get; set; }
        public string DepartureDay { get; set; }
        public string DepartureTime { get; set; }
        public string ArrivalMonth { get; set; }
        public string ArrivalDay { get; set; }
        public string ArrivalTime { get; set; }
        public string ToEmail { get; set; }
    }
}
