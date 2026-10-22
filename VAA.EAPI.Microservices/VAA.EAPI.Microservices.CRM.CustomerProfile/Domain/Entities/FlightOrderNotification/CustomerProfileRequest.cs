namespace VAA.EAPI.Microservices.CRM.CustomerProfile.Domain.Entities.FlightOrderNotification
{
    public class CustomerProfileRequest
    {
        public Passenger Passenger { get; set; }
        public string PnrLocator { get; set; }
    }
}
