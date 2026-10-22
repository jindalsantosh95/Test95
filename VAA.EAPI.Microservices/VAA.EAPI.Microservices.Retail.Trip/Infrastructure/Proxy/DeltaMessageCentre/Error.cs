namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaMessageCentre
{
    public class Error
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public DeveloperMessage DeveloperMessage { get; set; }
        public IEnumerable<MoreInfo> MoreInfo { get; set; }
        public ErrorCategoryType Category { get; set; }
    }
}
