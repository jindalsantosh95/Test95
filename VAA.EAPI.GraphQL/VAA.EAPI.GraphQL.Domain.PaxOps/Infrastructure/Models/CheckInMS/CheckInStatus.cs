namespace VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.CheckInMS
{
    public class OnlineCheckInStatus
    {
        public bool Available { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Code { get; set; }
        public string DeveloperMessage { get; set; }
        public string? AdvisoryText1 { get; set; }
    }
}
