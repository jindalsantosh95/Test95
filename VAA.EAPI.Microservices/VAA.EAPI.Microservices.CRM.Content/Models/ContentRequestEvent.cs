namespace VAA.EAPI.Microservices.CRM.Content.Models
{
    public class ContentRequestEvent
    {
        public string ContentFragmentId { get; set; }
        public string ReplyTopic { get; set; }
        public string ReplySubject { get; set; } = string.Empty;
    }
}
