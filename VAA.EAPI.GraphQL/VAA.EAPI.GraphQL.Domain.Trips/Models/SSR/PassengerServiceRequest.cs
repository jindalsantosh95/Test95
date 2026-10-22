namespace VAA.EAPI.GraphQL.Domain.Trips.Models.SSR
{
    public class PassengerServiceRequest
    {
        public string? CategoryCode { get; set; }
        public string? CategoryTitle { get; set; }
        public IEnumerable<CategoryDetail>? CategoryDetails { get; set; }
        public bool IsSegmentSelectEligible { get; set; }
        public bool IsServiceRequested { get; set; }
        public bool IsServiceChangeAllowed { get; set; }
    }
}


