namespace VAA.EAPI.Automation.Core.Delta.Models.OfferSearch
{
    public class Preferences
    {
        public bool nonStopOnly { get; set; }
        public List<string> permittedClassOfService { get; set; }
        public List<string> prohibitedClassOfService { get; set; }
        public bool refundableOnly { get; set; }
    }
}