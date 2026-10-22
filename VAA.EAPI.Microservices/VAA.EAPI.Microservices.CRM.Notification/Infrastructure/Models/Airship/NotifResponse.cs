namespace VAA.EAPI.Microservices.CRM.Notification.Infrastructure.Models.Airship
{
    public class NotifResponse
    {
        public bool Ok { get; set; }
        public string Operation_Id { get; set; }
        public IEnumerable<string> Push_Ids { get; set; }
        public string Error { get; set; }
        public int ErrorCode { get; set; }
        public ErrorDetails Details { get; set; }
    }

}
