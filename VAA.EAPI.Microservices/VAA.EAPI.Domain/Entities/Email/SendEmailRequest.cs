namespace VAA.EAPI.Domain.Entities.Email
{
    public class SendEmailRequest
    {
        public string MessageId { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Cc { get; set; }
        public string Bcc { get; set; }
        public string Subject { get; set; }
        public string Tag { get; set; }
        public string Channel { get; set; }
        public string LanguageCode { get; set; }
        public string Template { get; set; }
        public string HtmlBody { get; set; }
        public string TextBody { get; set; }
        public string ReplyTo { get; set; }
        public object DataFields { get; set; }
    }
}
