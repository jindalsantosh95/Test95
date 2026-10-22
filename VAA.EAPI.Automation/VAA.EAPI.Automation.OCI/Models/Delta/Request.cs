namespace VAA.EAPI.Automation.OCI.Models.Delta
{
    public class Request
    {
        public string passengerResourceId { get; set; }
        public string @using { get; set; }
        public IdentityDocument identityDocument { get; set; }
        public bool consent { get; set; }
        public bool modified { get; set; }
        public bool saveToProfile { get; set; }
    }
}