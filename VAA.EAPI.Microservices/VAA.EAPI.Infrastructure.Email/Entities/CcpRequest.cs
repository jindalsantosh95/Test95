namespace VAA.EAPI.Infrastructure.Email.Entities
{
    public class CcpRequest
    {
        public string CustomerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public string FromAddress { get; set; }
        public string Channel { get; set; }
        public string LanguageCode { get; set; }
        public string Template { get; set; }
        public MessageData MessageData { get; set; }
    }
}
