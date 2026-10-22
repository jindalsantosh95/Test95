namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.SSRMS
{
    public class ServiceRequest
    {
        public string ServiceTypeCode { get; set; }
        public string ServiceTypeName { get; set; }
        public string ServiceStatus { get; set; }
        public bool IsSelected { get; set; }
        public bool IsEligible { get; set; }
        public bool CanModify { get; set; }
        public string SelectedChoice { get; set; }
        public string ServiceCategory { get; set; }
        public IEnumerable<string> AvailableChoices { get; set; }
    }
}