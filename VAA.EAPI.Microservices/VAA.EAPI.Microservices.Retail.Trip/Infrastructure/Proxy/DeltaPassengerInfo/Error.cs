namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaPassengerInfo
{
    public class Error
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public string DeveloperMessage { get; set; }
        public IEnumerable<MoreInfo> MoreInfo { get; set; }
        public ErrorCategoryType Category { get; set; }
    }
}